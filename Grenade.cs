using GameNetcodeStuff;
using System.Collections;
using UnityEngine;

namespace FragGrenade.LCMod
{
    public class GrenadeItem : GrabbableObject
    {
        // Token: 0x06000F0A RID: 3850 RVA: 0x0007EB24 File Offset: 0x0007CD24
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (inPullingPinAnimation)
            {
                return;
            }
            if (pinPulled)
            {
                if (IsOwner)
                {
                    playerHeldBy.DiscardHeldObject(true, null, GetGrenadeThrowDestination(), true);
                }
                return;
            }
            if (pullPinCoroutine != null)
            {
                return;
            }
            playerHeldBy.activatingItem = true;
            pullPinCoroutine = StartCoroutine(pullPinAnimation());
        }

        // Token: 0x06000F0B RID: 3851 RVA: 0x0006F24E File Offset: 0x0006D44E
        public override void DiscardItem()
        {
            if (playerHeldBy != null)
            {
                playerHeldBy.activatingItem = false;
            }
            base.DiscardItem();
        }

        // Token: 0x06000F0C RID: 3852 RVA: 0x0007EB8E File Offset: 0x0007CD8E
        public override void EquipItem()
        {
            SetControlTipForGrenade();
            EnableItemMeshes(true);
            isPocketed = false;
        }

        // Token: 0x06000F0D RID: 3853 RVA: 0x0007EBA4 File Offset: 0x0007CDA4
        private void SetControlTipForGrenade()
        {
            string[] allLines;
            if (pinPulled)
            {
                allLines = new string[]
                {
                "Throw grenade: [RMB]"
                };
            }
            else
            {
                allLines = new string[]
                {
                "Pull pin: [RMB]"
                };
            }
            if (IsOwner)
            {
                HUDManager.Instance.ChangeControlTipMultiple(allLines, true, itemProperties);
            }
        }

        // Token: 0x06000F0E RID: 3854 RVA: 0x0007EBF4 File Offset: 0x0007CDF4
        public override void FallWithCurve()
        {
            float magnitude = (startFallingPosition - targetFloorPosition).magnitude;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(itemProperties.restingRotation.x, transform.eulerAngles.y, itemProperties.restingRotation.z), 14f * Time.deltaTime / magnitude);
            transform.localPosition = Vector3.Lerp(startFallingPosition, targetFloorPosition, grenadeFallCurve.Evaluate(fallTime));
            if (magnitude > 5f)
            {
                transform.localPosition = Vector3.Lerp(new Vector3(transform.localPosition.x, startFallingPosition.y, transform.localPosition.z), new Vector3(transform.localPosition.x, targetFloorPosition.y, transform.localPosition.z), grenadeVerticalFallCurveNoBounce.Evaluate(fallTime));
            }
            else
            {
                transform.localPosition = Vector3.Lerp(new Vector3(transform.localPosition.x, startFallingPosition.y, transform.localPosition.z), new Vector3(transform.localPosition.x, targetFloorPosition.y, transform.localPosition.z), grenadeVerticalFallCurve.Evaluate(fallTime));
            }
            fallTime += Mathf.Abs(Time.deltaTime * 12f / magnitude);
        }

        // Token: 0x06000F0F RID: 3855 RVA: 0x0007EDD6 File Offset: 0x0007CFD6
        private IEnumerator pullPinAnimation()
        {
            inPullingPinAnimation = true;
            playerHeldBy.activatingItem = true;
            playerHeldBy.doingUpperBodyEmote = 1.16f;
            playerHeldBy.playerBodyAnimator.SetTrigger(playerAnimation);
            itemAnimator.SetTrigger("pullPin");
            itemAudio.PlayOneShot(pullPinSFX);
            WalkieTalkie.TransmitOneShotAudio(itemAudio, pullPinSFX, 0.8f);
            yield return new WaitForSeconds(1f);
            if (playerHeldBy != null)
            {
                if (!DestroyGrenade)
                {
                    playerHeldBy.activatingItem = false;
                }
                playerThrownBy = playerHeldBy;
            }
            inPullingPinAnimation = false;
            pinPulled = true;
            itemUsedUp = true;
            if (IsOwner && playerHeldBy != null)
            {
                SetControlTipForGrenade();
            }
            yield break;
        }

        // Token: 0x06000F10 RID: 3856 RVA: 0x0007EDE8 File Offset: 0x0007CFE8
        public override void Update()
        {
            base.Update();
            if (pinPulled && !hasExploded)
            {
                explodeTimer += Time.deltaTime;
                if (explodeTimer > TimeToExplode)
                {
                    ExplodeStunGrenade(DestroyGrenade);
                }
            }
        }

        // Token: 0x06000F11 RID: 3857 RVA: 0x0007EE38 File Offset: 0x0007D038
        private void ExplodeStunGrenade(bool destroy = false)
        {
            if (hasExploded)
            {
                return;
            }
            hasExploded = true;
            itemAudio.PlayOneShot(explodeSFX);
            WalkieTalkie.TransmitOneShotAudio(itemAudio, explodeSFX, 1f);
            Transform parent;
            /*if (isInElevator)
            {
                parent = StartOfRound.Instance.elevatorTransform;
            }
            else
            {
                parent = RoundManager.Instance.mapPropsContainer.transform;
            }
            Instantiate(stunGrenadeExplosion, transform.position, Quaternion.identity, parent);*/
            StunExplosion(transform.position);
            if (DestroyGrenade)
            {
                DestroyObjectInHand(playerThrownBy);
            }
            Destroy(gameObject);
        }

        // Token: 0x06000F12 RID: 3858 RVA: 0x0007EF04 File Offset: 0x0007D104
        public static void StunExplosion(Vector3 explosionPosition)
        {
            Landmine.SpawnExplosion(explosionPosition, true, killRange: 8f, damageRange: 12f);
        }

        // Token: 0x06000F13 RID: 3859 RVA: 0x0007F184 File Offset: 0x0007D384
        public Vector3 GetGrenadeThrowDestination()
        {
            Vector3 vector = transform.position;
            Debug.DrawRay(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward, Color.yellow, 15f);
            grenadeThrowRay = new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward);
            if (Physics.Raycast(grenadeThrowRay, out grenadeHit, 12f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                vector = grenadeThrowRay.GetPoint(grenadeHit.distance - 0.05f);
            }
            else
            {
                vector = grenadeThrowRay.GetPoint(10f);
            }
            Debug.DrawRay(vector, Vector3.down, Color.blue, 15f);
            grenadeThrowRay = new Ray(vector, Vector3.down);
            Vector3 result;
            if (Physics.Raycast(grenadeThrowRay, out grenadeHit, 30f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                result = grenadeHit.point + Vector3.up * 0.05f;
            }
            else
            {
                result = grenadeThrowRay.GetPoint(30f);
            }
            return result;
        }

        // Token: 0x06000F15 RID: 3861 RVA: 0x0007F2F8 File Offset: 0x0007D4F8
        public override void __initializeVariables()
        {
            base.__initializeVariables();
        }

        // Token: 0x06000F16 RID: 3862 RVA: 0x0007F30E File Offset: 0x0007D50E
        public override string __getTypeName()
        {
            return "FragGrenadeItem";
        }

        // Token: 0x04000F0D RID: 3853
        [Header("Stun grenade settings")]
        public float TimeToExplode = 4.25f;

        // Token: 0x04000F0E RID: 3854
        public bool DestroyGrenade;

        // Token: 0x04000F0F RID: 3855
        public string playerAnimation = "PullGrenadePin";

        // Token: 0x04000F10 RID: 3856
        [Space(3f)]
        public bool pinPulled;

        // Token: 0x04000F11 RID: 3857
        public bool inPullingPinAnimation;

        // Token: 0x04000F12 RID: 3858
        private Coroutine pullPinCoroutine;

        // Token: 0x04000F13 RID: 3859
        public Animator itemAnimator;

        // Token: 0x04000F14 RID: 3860
        public AudioSource itemAudio;

        // Token: 0x04000F15 RID: 3861
        public AudioClip pullPinSFX;

        // Token: 0x04000F16 RID: 3862
        public AudioClip explodeSFX;

        // Token: 0x04000F17 RID: 3863
        public AnimationCurve grenadeFallCurve;

        // Token: 0x04000F18 RID: 3864
        public AnimationCurve grenadeVerticalFallCurve;

        // Token: 0x04000F19 RID: 3865
        public AnimationCurve grenadeVerticalFallCurveNoBounce;

        // Token: 0x04000F1A RID: 3866
        public RaycastHit grenadeHit;

        // Token: 0x04000F1B RID: 3867
        public Ray grenadeThrowRay;

        // Token: 0x04000F1C RID: 3868
        public float explodeTimer;

        // Token: 0x04000F1D RID: 3869
        public bool hasExploded;

        // Token: 0x04000F1E RID: 3870
        public GameObject stunGrenadeExplosion;

        // Token: 0x04000F1F RID: 3871
        private PlayerControllerB playerThrownBy;
    }
}

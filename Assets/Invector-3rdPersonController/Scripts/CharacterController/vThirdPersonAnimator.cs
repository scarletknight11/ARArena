using UnityEngine;
using System.Collections;

namespace Invector
{
    public abstract class vThirdPersonAnimator : vThirdPersonInput
    {
        #region Variables
        // generate a random idle animation
        private float randomIdleCount, randomIdle;
        // used to lerp the head track
        private Vector3 lookPosition;
        // match cursorObject to help animation to reach their cursorObject
        [HideInInspector]
        public Transform matchTarget;
        // head track control, if you want to turn off at some point, make it 0
        [HideInInspector]
        public float lookAtWeight;
        [HideInInspector]
        public float oldSpeed;
        public float speedTime
        {
            get
            {
                var _speed = animator.GetFloat("Speed");
                var acceleration = (_speed - oldSpeed) / Time.fixedDeltaTime;
                oldSpeed = _speed;
                return Mathf.Round(acceleration);
            }
        }
        private bool resetState;

        // get Layers from the Animator Controller
        [HideInInspector]
        public AnimatorStateInfo baseLayerInfo, underBodyInfo, rightArmInfo, leftArmInfo, fullBodyInfo, upperBodyInfo;

        int baseLayer { get { return animator.GetLayerIndex("Base Layer"); } }
        int underBodyLayer { get { return animator.GetLayerIndex("UnderBody"); } }
        int rightArmLayer { get { return animator.GetLayerIndex("RightArm"); } }
        int leftArmLayer { get { return animator.GetLayerIndex("LeftArm"); } }
        int upperBodyLayer { get { return animator.GetLayerIndex("UpperBody"); } }
        int fullbodyLayer { get { return animator.GetLayerIndex("FullBody"); } }

        #endregion
        
        protected void UpdateAnimator()
        {
            if (ragdolled)
                DisableActions();

            if (animator == null || !animator.enabled) return;

            LayerControl();

            RandomIdle();
            QuickTurn180Animation();
            RollAnimation();
            LandHighAnimation();
            JumpOverAnimation();
            ClimbUpAnimation();
            StepUpAnimation();
            JumpAnimation();
            LadderAnimation();
            TurnOnStopAnimation();
            QuickStopAnimation();
            HitReactionAnimation();
            HitRecoilAnimation();
            ATK_Animation();
            DEF_Animation();
            ExtraMoveSpeed();
            LocomotionAnimation();
            DeadAnimation();
        }              

        void LayerControl()
        {
            baseLayerInfo = animator.GetCurrentAnimatorStateInfo(baseLayer);
            underBodyInfo = animator.GetCurrentAnimatorStateInfo(underBodyLayer);
            rightArmInfo = animator.GetCurrentAnimatorStateInfo(rightArmLayer);
            leftArmInfo = animator.GetCurrentAnimatorStateInfo(leftArmLayer);
            upperBodyInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayer);
            fullBodyInfo = animator.GetCurrentAnimatorStateInfo(fullbodyLayer);                      
        }

        void OnAnimatorIK()
        {
            HeadTrackIK();
            DragBoxIK();
        }        

        #region Locomotion Animations

        /// <summary>
        /// RANDOM IDLE	- assign the animations at the Layer IdleVariations on the Animator	
        /// </summary>
        void RandomIdle()
        {
            if (randomIdleTime > 0)
            {
                if (input.sqrMagnitude == 0 && !strafing && !crouch && _capsuleCollider.enabled && onGround)
                {
                    randomIdleCount += Time.fixedDeltaTime;
                    if (randomIdleCount > 6)
                    {
                        randomIdleCount = 0;
                        animator.SetTrigger("IdleRandomTrigger");
                        animator.SetInteger("IdleRandom", Random.Range(1, 4));
                    }
                }
                else
                {
                    randomIdleCount = 0;
                    animator.SetInteger("IdleRandom", 0);
                }
            }
        }
        
        void LocomotionAnimation()
        {
            animator.SetBool("Strafing", strafing);
            animator.SetBool("Crouch", crouch);
            animator.SetBool("OnGround", onGround);
            animator.SetFloat("GroundDistance", groundDistance);
            animator.SetFloat("VerticalVelocity", verticalVelocity);

            if(meleeManager != null)
                animator.SetFloat("MoveSet_ID", combatID.moveSet, 0.1f, Time.fixedDeltaTime);

            if (freeLocomotionConditions)
                // free directional movement get the directional angle
                animator.SetFloat("Direction", lockMovement ? 0f : direction, 0.1f, Time.fixedDeltaTime);
            else
                // strafe movement get the input 1 or -1
                animator.SetFloat("Direction", lockMovement ? 0f : direction, 0.25f, Time.fixedDeltaTime);

            animator.SetFloat("Speed", !stopMove || lockMovement ? speed : 0f, 0.2f, Time.fixedDeltaTime);
        }

        /// <summary>
        /// EXTRA MOVE SPEED - for NON-RootMotion or just apply extra speed to increase movement speed
        /// </summary>
        void ExtraMoveSpeed()
        {
            if (stopMove) return;
            if (!inAttack)
            {
                // strafe extra speed
                if (baseLayerInfo.IsName("Grounded.Strafing Movement"))
                {
                    var newSpeed_Y = (strafeSpeed * speed);
                    var newSpeed_X = (strafeSpeed * direction);
                    newSpeed_Y = Mathf.Clamp(newSpeed_Y, -strafeSpeed, strafeSpeed);
                    newSpeed_X = Mathf.Clamp(newSpeed_X, -strafeSpeed, strafeSpeed);
                    _rigidbody.AddForce(transform.forward * (newSpeed_Y), ForceMode.VelocityChange);
                    _rigidbody.AddForce(transform.right * (newSpeed_X), ForceMode.VelocityChange);
                }
                // crouch strafe extra speed
                else if(baseLayerInfo.IsName("Grounded.Strafing Crouch"))
                {
                    var newSpeed_Y = (crouchStrafeSpeed * speed);
                    var newSpeed_X = (crouchStrafeSpeed * direction);
                    newSpeed_Y = Mathf.Clamp(newSpeed_Y, -crouchStrafeSpeed, crouchStrafeSpeed);
                    newSpeed_X = Mathf.Clamp(newSpeed_X, -crouchStrafeSpeed, crouchStrafeSpeed);
                    _rigidbody.AddForce(transform.forward * (newSpeed_Y), ForceMode.VelocityChange);
                    _rigidbody.AddForce(transform.right * (newSpeed_X), ForceMode.VelocityChange);
                }
                else if (baseLayerInfo.IsName("Grounded.Free Movement"))
                {
                    // walk extra speed                    
                    if(speed <= 0.5f)
                        _rigidbody.AddForce(transform.forward * (walkSpeed * speed), ForceMode.VelocityChange);
                    // running extra speed
                    else if(speed > 0.5 && speed <= 1f)
                        _rigidbody.AddForce(transform.forward * (runningSpeed * speed), ForceMode.VelocityChange);
                    // sprint extra speed
                    else
                        _rigidbody.AddForce(transform.forward * (sprintSpeed * speed), ForceMode.VelocityChange);
                }
                //crouch extra speed
                else if(baseLayerInfo.IsName("Grounded.Free Crouch"))                
                    _rigidbody.AddForce(transform.forward * (crouchSpeed * speed), ForceMode.VelocityChange);
            }
            else            
                speed = 0f;            
        }

        /// <summary>
        /// Trigger Dead animation
        /// </summary>
        void DeadAnimation()
        {
            if (!isDead) return;
            // lock the player input
            lockMovement = true;
            // change the culling mode to render the animation until it finish
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            // disable any actions to prevent any transitions
            DisableActions();
            // reset all states just in case the characters is attacking
            animator.SetTrigger("ResetState");

            // death by animation
            if (deathBy == DeathBy.Animation)
            {
                animator.SetBool("Dead", true);
                if (baseLayerInfo.IsName("Action.Dead"))
                {
                    if (baseLayerInfo.normalizedTime >= 0.99f)
                        RemoveComponents();
                }
            }
            // death by animation & ragdoll after a time
            else if (deathBy == DeathBy.AnimationWithRagdoll)
            {
                animator.SetBool("Dead", true);
                if (baseLayerInfo.IsName("Action.Dead"))
                {
                    // activate the ragdoll after the animation finish played
                    if (baseLayerInfo.normalizedTime >= 0.8f)
                        SendMessage("ActivateRagdoll", SendMessageOptions.DontRequireReceiver);
                }
            }
            // death by ragdoll
            else if (deathBy == DeathBy.Ragdoll)
                SendMessage("ActivateRagdoll", SendMessageOptions.DontRequireReceiver);
        }

        void QuickTurn180Animation()
        {
            animator.SetBool("QuickTurn180", quickTurn);

            // complete the 180 with matchTarget and disable quickTurn180 after completed
            if (baseLayerInfo.IsName("Action.QuickTurn180"))
            {
                if (!animator.IsInTransition(0) && !ragdolled)
                    animator.MatchTarget(Vector3.one, freeRotation, AvatarTarget.Root,
                                 new MatchTargetWeightMask(Vector3.zero, 1f),
                                 animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 0.9f);

                if (baseLayerInfo.normalizedTime >= 0.9f || roll)
                    quickTurn = false;
            }
        }

        void TurnOnStopAnimation()
        {
            if (!turnOnSpot) return;
            var rot = Quaternion.LookRotation(lookPoint(1000f) - transform.position, Vector3.up);
            var newAngle = rot.eulerAngles - transform.eulerAngles;
            var ang = newAngle.NormalizeAngle().y;

            if (strafing && combatID.moveSet == 0 && !actions && onGround)
                animator.SetFloat("StrafeAngle", ang);
            else
                animator.SetFloat("StrafeAngle", 0);
        }

        void QuickStopAnimation()
        {
            if (!quickStopAnim) return;

            animator.SetBool("QuickStop", quickStop);

            bool quickStopConditions = !actions && onGround && !inAttack;	       
            // make a quickStop when release the button while running
            if (speedTime <= -6f && quickStopConditions)
                quickStop = true;           

            // disable quickStop
            if (quickStop && input.sqrMagnitude >= 0.1f || quickTurn || inAttack)
                quickStop = false;
            else if (baseLayerInfo.IsName("Action.QuickStop"))
            {
                if (baseLayerInfo.normalizedTime > 0.9f || input.sqrMagnitude >= 0.1f || stopMove || inAttack)
                    quickStop = false;
            }
        }
        
        void LadderAnimation()
        {
            // resume the states of the ladder in one bool 
            usingLadder =
                baseLayerInfo.IsName("Ladder.EnterLadderBottom") ||
                baseLayerInfo.IsName("Ladder.ExitLadderBottom") ||
                baseLayerInfo.IsName("Ladder.ExitLadderTop") ||
                baseLayerInfo.IsName("Ladder.EnterLadderTop") ||
                baseLayerInfo.IsName("Ladder.ClimbLadder");

            LadderBottom();
            LadderTop();
        }

        void LadderBottom()
        {
            animator.SetBool("EnterLadderBottom", enterLadderBottom);
            animator.SetBool("ExitLadderBottom", exitLadderBottom);

            // enter ladder from bottom
            if (baseLayerInfo.IsName("Ladder.EnterLadderBottom"))
            {
                _capsuleCollider.isTrigger = true;
                _rigidbody.useGravity = false;

                // we are using matchtarget to find the correct X & Z to start climb the ladder
                // this information is provided by the cursorObject on the object, that use the script TriggerAction 
                // in this state we are sync the position based on the AvatarTarget.Root, but you can use leftHand, left Foot, etc.
                if (!animator.IsInTransition(0))
                    animator.MatchTarget(matchTarget.position, matchTarget.rotation,
                               AvatarTarget.Root, new MatchTargetWeightMask
                                (new Vector3(1, 1, 1), 1), 0.25f, 0.9f);

                if (baseLayerInfo.normalizedTime >= 0.75f || hitReaction)
                    enterLadderBottom = false;
            }

            // exit ladder bottom
            if (baseLayerInfo.IsName("Ladder.ExitLadderBottom") || hitReaction)
            {
                _capsuleCollider.isTrigger = false;
                _rigidbody.useGravity = true;

                if (baseLayerInfo.normalizedTime >= 0.4f || hitReaction)
                {
                    exitLadderBottom = false;
                    usingLadder = false;
                }
            }
        }

        void LadderTop()
        {
            animator.SetBool("EnterLadderTop", enterLadderTop);
            animator.SetBool("ExitLadderTop", exitLadderTop);

            // enter ladder from top            
            if (baseLayerInfo.IsName("Ladder.EnterLadderTop"))
            {
                _capsuleCollider.isTrigger = true;
                _rigidbody.useGravity = false;

                // we are using matchtarget to find the correct X & Z to start climb the ladder
                // this information is provided by the cursorObject on the object, that use the script TriggerAction 
                // in this state we are sync the position based on the AvatarTarget.Root, but you can use leftHand, left Foot, etc.
                if (baseLayerInfo.normalizedTime < 0.25f && !animator.IsInTransition(0))
                    animator.MatchTarget(matchTarget.position, matchTarget.rotation,
                                AvatarTarget.Root, new MatchTargetWeightMask
                                (new Vector3(1, 0, 0.1f), 1), 0f, 0.25f);
                else if (!animator.IsInTransition(0))
                    animator.MatchTarget(matchTarget.position, matchTarget.rotation,
                                AvatarTarget.Root, new MatchTargetWeightMask
                                (new Vector3(1, 1, 1), 1), 0.25f, 0.7f);

                if (baseLayerInfo.normalizedTime >= 0.7f || hitReaction)
                    enterLadderTop = false;
            }

            // exit ladder top
            if (baseLayerInfo.IsName("Ladder.ExitLadderTop") || hitReaction)
            {
                if (baseLayerInfo.normalizedTime >= 0.85f || hitReaction)
                {
                    _capsuleCollider.isTrigger = false;
                    _rigidbody.useGravity = true;
                    exitLadderTop = false;
                    usingLadder = false;
                }
            }
        }
        
        void RollAnimation()
        {
            animator.SetBool("Roll", roll);

            bool lockRollDir = baseLayerInfo.IsName("Action.Roll") && baseLayerInfo.normalizedTime <= 0.9f && !animator.IsInTransition(0);

            if (roll && strafing && !lockRollDir && (input != Vector2.zero || speed > 0.25f))
            {
                // check the right direction for rolling if you are strafing
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDirection, freeRotationSpeed * Time.fixedDeltaTime, 0.0f);
                freeRotation = Quaternion.LookRotation(newDir);
                var eulerAngles = new Vector3(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z);
                transform.eulerAngles = eulerAngles;
            }

            isRolling = baseLayerInfo.IsName("Action.Roll");
            if (baseLayerInfo.IsName("Action.Roll"))
            {
                lockMovement = true;
                _rigidbody.useGravity = false;                

                // prevent the character to rolling up 
                if (verticalVelocity >= 1)
                    _rigidbody.velocity = Vector3.ProjectOnPlane(_rigidbody.velocity, groundHit.normal);                

                // reset the rigidbody a little ealier to the character fall while on air
                if (baseLayerInfo.normalizedTime > 0.3f)
                    _rigidbody.useGravity = true;

                // transition back if the character is not crouching
                if (!crouch && baseLayerInfo.normalizedTime >= 0.85f)
                {
                    lockMovement = false;
                    roll = false;
                }
                // transition back if the character is crouching
                else if (crouch && baseLayerInfo.normalizedTime > 0.75f)
                {
                    lockMovement = false;
                    roll = false;                    
                }
            }
        }        	    
        
        void JumpAnimation()
        {
            isJumping = baseLayerInfo.IsName("Action.Jump") || baseLayerInfo.IsName("Action.JumpMove") || baseLayerInfo.IsName("Airborne.FallingFromJump");

            animator.SetBool("Jump", jump);                      
            animator.SetBool("IsJumping", isJumping);
            
            if (baseLayerInfo.IsName("Action.Jump"))
                JumpStateBehaviour(0.85f, 0.6f);

            if (baseLayerInfo.IsName("Action.JumpMove"))            
                JumpStateBehaviour(0.85f, 0.55f);            
           
            if (baseLayerInfo.IsName("Airborne.FallingFromJump"))
                JumpControl();
        }

        void JumpStateBehaviour(float startNormalizedTime, float endNormalizedTime)
        {
            jumpForce = actionsController.Jump.jumpForce;
            jumpForward = actionsController.Jump.jumpForward;
            jumpAirControl = actionsController.Jump.jumpAirControl;

            // apply extra force to the jump
            if (baseLayerInfo.normalizedTime < startNormalizedTime)
                _rigidbody.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);

            JumpControl();

            // end jump animation
            if (baseLayerInfo.normalizedTime >= endNormalizedTime || hitReaction)
                jump = false;
        }       

        void LandHighAnimation()
        {
            animator.SetBool("LandHigh", landHigh);

            // if the character fall from a great height, landhigh animation
            if (!onGround && verticalVelocity <= landHighVel && groundDistance <= 0.5f)
                landHigh = true;

            if (landHigh && baseLayerInfo.IsName("Airborne.LandHigh"))
            {
                quickStop = false;
                if (baseLayerInfo.normalizedTime >= 0.1f && baseLayerInfo.normalizedTime <= 0.2f)
                {
                    // vibrate the controller 
                    #if UNITY_STANDALONE_WIN || UNITY_EDITOR
                    StartCoroutine(GamepadVibration(0.15f));
	                #endif
                }

                if (baseLayerInfo.normalizedTime > 0.9f || hitReaction)
                {
                    landHigh = false;
                }
            }
        }

        #endregion

        #region Action Animations

        void StepUpAnimation()
        {
            animator.SetBool("StepUp", stepUp);

            if (baseLayerInfo.IsName("Action.StepUp"))
            {
                if (baseLayerInfo.normalizedTime > 0.1f && baseLayerInfo.normalizedTime < 0.3f)
                {
                    _capsuleCollider.isTrigger = true;
                    _rigidbody.useGravity = false;
                }

                // we are using matchtarget to find the correct height of the object                
                if (!animator.IsInTransition(0))
                    animator.MatchTarget(matchTarget.position, matchTarget.rotation,
                                AvatarTarget.LeftHand, new MatchTargetWeightMask
                                (new Vector3(0, 1, 1), 0), 0f, 0.5f);

                if (baseLayerInfo.normalizedTime > 0.9f || hitReaction)
                {
                    _capsuleCollider.isTrigger = false;
                    _rigidbody.useGravity = true;
                    stepUp = false;
                }
            }
        }
        
        void JumpOverAnimation()
        {
            animator.SetBool("JumpOver", jumpOver);

            if (baseLayerInfo.IsName("Action.JumpOver"))
            {
                quickTurn = false;
                if (baseLayerInfo.normalizedTime > 0.1f && baseLayerInfo.normalizedTime < 0.3f)
                {
                    _rigidbody.useGravity = false;
                    _capsuleCollider.isTrigger = true;
                }

                // we are using matchtarget to find the correct height of the object
                if (!animator.IsInTransition(0))
                    animator.MatchTarget(matchTarget.position, matchTarget.rotation,
                                AvatarTarget.LeftHand, new MatchTargetWeightMask
                                (new Vector3(0, 1, 1), 0), 0.1f * (1 - baseLayerInfo.normalizedTime), 0.3f * (1 - baseLayerInfo.normalizedTime));

                if (baseLayerInfo.normalizedTime >= 0.7f || hitReaction)
                {
                    _rigidbody.useGravity = true;
                    _capsuleCollider.isTrigger = false;
                    jumpOver = false;
                }
            }
        }
        
        void ClimbUpAnimation()
        {
            animator.SetBool("ClimbUp", climbUp);

            if (baseLayerInfo.IsName("Action.ClimbUp"))
            {
                if (baseLayerInfo.normalizedTime > 0.1f && baseLayerInfo.normalizedTime < 0.3f)
                {
                    _rigidbody.useGravity = false;
                    _capsuleCollider.isTrigger = true;
                }

                // we are using matchtarget to find the correct height of the object
                if (!animator.IsInTransition(0))
                    animator.MatchTarget(matchTarget.position, matchTarget.rotation,
                               AvatarTarget.LeftHand, new MatchTargetWeightMask
                               (new Vector3(0, 1, 1), 0), 0f, 0.2f);

                if (crouch)
                {
                    if (baseLayerInfo.normalizedTime >= 0.7f || hitReaction)
                    {
                        _capsuleCollider.isTrigger = false;
                        _rigidbody.useGravity = true;
                        climbUp = false;
                    }
                }
                else
                {
                    if (baseLayerInfo.normalizedTime >= 0.9f || hitReaction)
                    {
                        _capsuleCollider.isTrigger = false;
                        _rigidbody.useGravity = true;
                        climbUp = false;
                    }
                }
            }
        }

        #endregion

        #region Melee Combat Animations

        /// <summary>
        /// HIT REACTION - it's activate on the TakeDamage() method by a trigger
        /// </summary>
        void HitReactionAnimation()
        {
            hitReaction = baseLayerInfo.IsTag("HitReaction");

            if (hitReaction)
                jump = false; isJumping = false;
        }

        /// <summary>
        /// HIT RECOIL - trigger a recoil animation when you hit a wall, check the RecoilReaction script at the weapon
        /// </summary>
        void HitRecoilAnimation()
        {
            hitRecoil = baseLayerInfo.IsTag("HitRecoil");
        }

        /// <summary>
        /// Trigger Recoil Animation - It's Called at the MeleeWeapon script using SendMessage
        /// </summary>
        public void TriggerRecoil(int recoil_id)
        {
            if (animator == null) return;
            animator.SetTrigger("ResetState");
            animator.SetInteger("Recoil_ID", recoil_id);
            animator.SetTrigger("HitRecoil");
        }

        /// <summary>
        /// ATTACK MELEE ANIMATION - trigger the attack animation based on the ID of your weapon
        /// </summary>
        void ATK_Animation()
        {
            if (meleeManager == null || animator == null) return;

            animator.SetBool("InAttack", inAttack);
            animator.SetInteger("ATK_ID", combatID.atk);
        }

        /// <summary>
        /// Reset the TriggerAttack to avoid keeping attacking when finish the combat, check on the Melee Attack Behaviour on the AnimatorState last animation
        /// </summary>
        public void ResetTrigger()
        {
            if (animator == null) return;
            animator.ResetTrigger("MeleeAttack");
        }

        /// <summary>
        /// DEFENSE ANIMATION - Trigger a defense animation based on the ID of your weapon
        /// </summary>
        void DEF_Animation()
        {
            if (meleeManager == null || animator == null) return;
            if (blocking)
                animator.SetInteger("DEF_ID", combatID.def);
            else
                animator.SetInteger("DEF_ID", 0);

            animator.SetBool("Mirror", combatID.mirror);
        }

        #endregion

        #region IK Routine

        Vector3 lookPoint(float distance)
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            var point = ray.GetPoint(distance);
            if (tpCamera != null && tpCamera.lockTarget != null)
                point = tpCamera.lockTarget.GetPointOffBoundsCenter(0.2f);

            return point;
        }
        
        void HeadTrackIK()
        {
            if (headWeight == 0f) return;

            var rot = Quaternion.LookRotation(lookPoint(1000f) - transform.position, Vector3.up);
            var newAngle = rot.eulerAngles - transform.eulerAngles;
            var ang = newAngle.NormalizeAngle().y;
            var angLimit = ang <= maxAngle && ang >= 0 || ang > -maxAngle && ang <= 0;            

            // turn on the headtrack on the states with the tag HeadTrack
            bool headTrackWhileAttacking = inAttack ? (baseLayerInfo.IsTag("HeadTrack") && fullBodyInfo.IsTag("HeadTrack")) : baseLayerInfo.IsTag("HeadTrack");
            // go to your Animator Controller and apply the Tag "HeadTrack" on the States that you want the character to have the headtrack effect
            bool headTrackConditions = angLimit && !actions && !lockMovement && headTrackWhileAttacking;

            if (headTrackConditions && !draggableBox)
                lookAtWeight += headTrackMultiplier * Time.deltaTime;
            else
                lookAtWeight -= headTrackMultiplier * Time.deltaTime;

            lookAtWeight = Mathf.Clamp(lookAtWeight, 0f, 1f);
            if (strafing)
                animator.SetLookAtWeight(lookAtWeight, strafeBodyWeight, headWeight, 1f);
            else
                animator.SetLookAtWeight(lookAtWeight, freeBodyWeight, headWeight, 1f);

            lookPosition = Vector3.Lerp(lookPosition, lookPoint(1000f), 10f * Time.fixedDeltaTime);
            animator.SetLookAtPosition(lookPosition);
        }

        void DragBoxIK()
        {
            if (draggableBox)
            {
                if (draggableBox.box.inDrag)
                    handIKWeight += draggableBox.IKLerp * Time.deltaTime;

                handIKWeight = Mathf.Clamp(handIKWeight, 0f, 1f);

                if (draggableBox.IKRightHand != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handIKWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, handIKWeight);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, draggableBox.IKRightHand.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, draggableBox.IKRightHand.rotation);
                }
                if (draggableBox.IKLeftHand != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handIKWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handIKWeight);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, draggableBox.IKLeftHand.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, draggableBox.IKLeftHand.rotation);
                }
                if (!dragStart)
                {
                    dragStart = true;
                    dragEuler = draggableBox.box.transform.eulerAngles - transform.eulerAngles;
                }
            }
            else
                handIKWeight = 0;
        }

        #endregion

    }    
}
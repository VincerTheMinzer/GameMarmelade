using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonCharacter : MonoBehaviour
	{
		[SerializeField] float _MovingTurnSpeed = 360;
		[SerializeField] float _StationaryTurnSpeed = 180;
		[SerializeField] float _JumpPower = 12f;
		[Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
		[SerializeField] float _RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float _MoveSpeedMultiplier = 1f;
		[SerializeField] float _AnimSpeedMultiplier = 1f;
		[SerializeField] float _GroundCheckDistance = 0.1f;

		Rigidbody _Rigidbody;
		Animator _Animator;
		bool _IsGrounded;
		float _OrigGroundCheckDistance;
		const float k_Half = 0.5f;
		float _TurnAmount;
		float _ForwardAmount;
		Vector3 _GroundNormal;
		float _CapsuleHeight;
		Vector3 _CapsuleCenter;
		CapsuleCollider _Capsule;
		bool _Crouching; 


		void Start()
		{
			_Animator = GetComponent<Animator>();
			_Rigidbody = GetComponent<Rigidbody>();
			_Capsule = GetComponent<CapsuleCollider>();
			_CapsuleHeight = _Capsule.height;
			_CapsuleCenter = _Capsule.center;

			_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			_OrigGroundCheckDistance = _GroundCheckDistance;
		}


		public void Move(Vector3 move, bool crouch, bool jump)
		{

			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f) move.Normalize();
			move = transform.InverseTransformDirection(move);
			CheckGroundStatus();
			move = Vector3.ProjectOnPlane(move, _GroundNormal);
			_TurnAmount = Mathf.Atan2(move.x, move.z);
			_ForwardAmount = move.z;

			ApplyExtraTurnRotation();

			// control and velocity handling is different when grounded and airborne:
			if (_IsGrounded)
			{
				HandleGroundedMovement(crouch, jump);
			}
			else
			{
				HandleAirborneMovement();
			}

			ScaleCapsuleForCrouching(crouch);
			PreventStandingInLowHeadroom();

			// send input and other state parameters to the animator
			Debug.Log(move);
			UpdateAnimator(move);
		}

		
		void ScaleCapsuleForCrouching(bool crouch)
		{
			if (_IsGrounded && crouch)
			{
				if (_Crouching) return;
				_Capsule.height = _Capsule.height / 2f;
				_Capsule.center = _Capsule.center / 2f;
				_Crouching = true;
			}
			else
			{
				Ray crouchRay = new Ray(_Rigidbody.position + Vector3.up * _Capsule.radius * k_Half, Vector3.up);
				float crouchRayLength = _CapsuleHeight - _Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, _Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
				{
					_Crouching = true;
					return;
				}
				_Capsule.height = _CapsuleHeight;
				_Capsule.center = _CapsuleCenter;
				_Crouching = false;
			}
		}

		void PreventStandingInLowHeadroom()
		{
			// prevent standing up in crouch-only zones
			if (!_Crouching)
			{
				Ray crouchRay = new Ray(_Rigidbody.position + Vector3.up * _Capsule.radius * k_Half, Vector3.up);
				float crouchRayLength = _CapsuleHeight - _Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, _Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
				{
					_Crouching = true;
				}
			}
		}
		

		void UpdateAnimator(Vector3 move)
		{
			// update the animator parameters
			_Animator.SetFloat("Forward", _ForwardAmount, 0.1f, Time.deltaTime);
			_Animator.SetFloat("Turn", _TurnAmount, 0.1f, Time.deltaTime);
			_Animator.SetBool("Crouch", _Crouching);
			_Animator.SetBool("OnGround", _IsGrounded);
			if (!_IsGrounded)
			{
				_Animator.SetFloat("Jump", _Rigidbody.velocity.y);
			}

			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + _RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * _ForwardAmount;
			if (_IsGrounded)
			{
				_Animator.SetFloat("JumpLeg", jumpLeg);
			}

			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (_IsGrounded && move.magnitude > 0)
			{
				_Animator.speed = _AnimSpeedMultiplier;
			}
			else
			{
				// don't use that while airborne
				_Animator.speed = 1;
			}
		}


		void HandleAirborneMovement()
		{
			// apply extra gravity from multiplier:
			Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
			_Rigidbody.AddForce(extraGravityForce);

			_GroundCheckDistance = _Rigidbody.velocity.y < 0 ? _OrigGroundCheckDistance : 0.01f;
		}


		void HandleGroundedMovement(bool crouch, bool jump)
		{
			// check whether conditions are right to allow a jump:
			if (jump && !crouch && _Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
			{
				// jump!
				_Rigidbody.velocity = new Vector3(_Rigidbody.velocity.x, _JumpPower, _Rigidbody.velocity.z);
				_IsGrounded = false;
				_Animator.applyRootMotion = false;
				_GroundCheckDistance = 0.1f;
			}
		}

		
		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(_StationaryTurnSpeed, _MovingTurnSpeed, _ForwardAmount);
			transform.Rotate(0, _TurnAmount * turnSpeed * Time.deltaTime, 0);
		}
		

		public void OnAnimatorMove()
		{
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			if (_IsGrounded && Time.deltaTime > 0)
			{
				Vector3 v = (_Animator.deltaPosition * _MoveSpeedMultiplier) / Time.deltaTime;

				// we preserve the existing y part of the current velocity.
				v.y = _Rigidbody.velocity.y;
				_Rigidbody.velocity = v;
			}
		}


		void CheckGroundStatus()
		{
			RaycastHit hitInfo;
#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * _GroundCheckDistance));
#endif
			// 0.1f is a small offset to start the ray from inside the character
			// it is also good to note that the transform position in the sample assets is at the base of the character
			if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, _GroundCheckDistance))
			{
				_GroundNormal = hitInfo.normal;
				_IsGrounded = true;
				_Animator.applyRootMotion = true;
			}
			else
			{
				_IsGrounded = false;
				_GroundNormal = Vector3.up;
				_Animator.applyRootMotion = false;
			}
		}
	}
}

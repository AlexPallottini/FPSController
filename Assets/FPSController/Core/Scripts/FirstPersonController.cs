using UnityEngine;
using Cinemachine;
using System.Collections;
using System;

namespace FPController
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerControlInputs), typeof(AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Modules"), Space]
        [SerializeField] private bool canSprint = true;
        [SerializeField] private bool canJump = true;
        [SerializeField] private bool canCrouch = true;
        [SerializeField] private bool headbobEnabled = true;
        [SerializeField] private bool willSlideOnSlopes = true;
        [SerializeField] private bool canZoom = true;
        [SerializeField] private bool canInteract = true;
        [SerializeField] private bool useFootsteps = true;
        [SerializeField] private bool useHealthSystem = true;
        [SerializeField] private bool useStaminaSystem = true;

        //General control variable for intern code reasons, setting this to false would be equal to deactivating almost all of the modules
        public bool CanMove { get; private set; } = true;

        [Header("Movement parameters")]
        [SerializeField, Min(0f), Tooltip("Default movement speed of the controller")] private float defaultSpeed = 3f;
        [SerializeField, Min(0f), Tooltip("Sprinting movement speed of the controller")] private float sprintSpeed = 6f;
        [SerializeField, Min(0f), Tooltip("Crouching movement speed of the controller")] private float crouchSpeed = 1.5f;
        [SerializeField, Min(0f), Tooltip("Speed of the character sliding down a slope")] private float slopeSlideSpeed = 8f;
        private CharacterController characterController;
        private Vector3 moveDirection;
        private Vector2 currentInput;

        private bool IsSprinting => canSprint && playerInputs.Sprint;
        private bool ShouldJump => playerInputs.Jump && characterController.isGrounded && !IsSliding;
        private bool ShouldCrouch => playerInputs.Crouch && !duringCrouchAnimation && characterController.isGrounded;


        [Header("Look parameters")]
        [SerializeField, Range(1, 10), Tooltip("Speed of the movement of the camera on the X axis")] private float lookSpeedHorizontalAxis = 2f;
        [SerializeField, Range(1, 10), Tooltip("Speed of the movement of the camera on the Y axis")] private float lookSpeedVerticalAxis = 2f;
        [SerializeField, Range(1, 180), Tooltip("Limit (On degrees) on how far up can the controller see")] private float upperLookLimitDegrees = 80f;
        [SerializeField, Range(1, 180), Tooltip("Limit (On degrees) on how far down can the controller see")] private float lowerLookLimitDegrees = 80f;
        private CinemachineVirtualCamera playerCamera;
        private float xRotation = 0f;

        [Header("Health parameters")]
        [SerializeField, Min(0f), Tooltip("Max health of the character controller")] private float maxHeatlh = 100f;
        [SerializeField, Min(0f), Tooltip("How much time has to pass before the controller starts regenerating health")] private float timeBeforeRegenStarts = 3f;
        [SerializeField, Min(0f), Tooltip("How fast should the healing 'ticks' should happen")] private float healthTimeIncrement = 0.1f;
        [SerializeField, Min(0f), Tooltip("By how much each of the 'ticks' heal")] private float healthValueIncrement = 1f;
        private float currentHealth;
        private Coroutine regeneratingHealth;
        public static Action<float> OnTakeDamage;
        public static Action<float> OnDamage;
        public static Action<float> OnHeal;

        [Header("Stamina parameters")]
        [SerializeField, Min(0f), Tooltip("Max stamina of the character controller")] private float maxStamina = 100f;
        [SerializeField, Min(0f),  Tooltip("How much stamina does sprint use")] private float staminaUseMultiplier = 5f;
        [SerializeField, Min(0f), Tooltip("How much time has to pass before the controller starts regenerating stamina")] private float timeBeforeStaminaRegenStarts = 5f;
        [SerializeField, Min(0f), Tooltip("How fast the regenerating 'ticks' should happen")] private float staminaTimeIncrement = 0.1f;
        [SerializeField, Min(0f), Tooltip("By how much each of the 'ticks' regenerate stamina")] private float staminaValueIncrement = 2f;
        private float currentStamina;
        private Coroutine regeneratingStamina;
        public static Action<float> OnStaminaChange;

        [Header("Jumping parameters")]
        [SerializeField, Min(0f), Tooltip("The force of the upwards movement that involves jumping")] private float jumpForce = 8f;
        [SerializeField, Min(0f), Tooltip("How much does gravity affect the controller (Unrelated to global gravity settings)")] private float gravity = 30f;

        [Header("Crouch parameters")]
        [SerializeField, Min(0f), Tooltip("The height of the character controller component when crouching")] private float crouchingHeight = 0.5f;
        [SerializeField, Min(0f), Tooltip("The height of the character controller component when standing")] private float standingHeight = 2f;
        [SerializeField, Range(0.01f, 10f), Tooltip("How much time it takes to switch between standing and crouching")] private float timeToCrouch = 0.25f;
        [SerializeField, Tooltip("The center of the character controller component when crouching")] private Vector3 crouchingCenter = new(0f,0.5f,0f);
        [SerializeField, Tooltip("The center of the character controller component when standing")] private Vector3 standingCenter = new(0f,0f,0f);
        private bool isCrouching;
        private bool duringCrouchAnimation;

        [Header("Headbob parameters")]
        [Space]
        [SerializeField, Tooltip("The default speed of the headbob movement")] private float defaultBobSpeed = 14f;
        [SerializeField, Tooltip("The default amount of the headbob movement")] private float defaultBobAmount = 0.05f;
        [Space]
        [SerializeField, Tooltip("The sprinting speed of the headbob movement")] private float sprintBobSpeed = 18f;
        [SerializeField, Tooltip("The sprinting amount of the headbob movement")] private float sprintBobAmount = 0.1f;
        [Space]
        [SerializeField, Tooltip("The crouching speed of the headbob movement")] private float crouchBobSpeed = 8f;
        [SerializeField, Tooltip("The crouching amount of the headbob movement")] private float crouchBobAmount = 0.025f;
        private float defaultYPosition = 0f;
        private float headbobTimer = 0f;

        [Header("Zoom parameters")]
        [SerializeField, Tooltip("How fast is the zoom in/out movement")] private float timeToZoom = 0.3f;
        [SerializeField, Tooltip("The target of the camera FOV when zoomed")] private float zoomFOV = 30f;
        private float defaultFOV;
        private Coroutine zoomRoutine;

        [Header("Interaction")]
        [SerializeField, Tooltip("Vector used in the Camera.ViewportPointToRay to calculate the hit of the interactable")] private Vector3 interactionRayPoint = default;
        [SerializeField, Min(0f),Tooltip("How far an interactable must be to be considered out of range")] private float interactionDistance = default;
        [SerializeField, Tooltip("LayerMask of the Interactable")] private LayerMask interactionLayer = default;
        private Interactable currentInteractable = null;

        [Header("Footstep parameters")]
        [SerializeField, Min(0f), Tooltip("Timer of the default speed when making the step sound")] private float baseStepSpeed = 0.5f;
        [SerializeField, Min(0f), Tooltip("Timer of the crouching speed when making the step sound")] private float crouchStepMultiplier = 1.5f;
        [SerializeField, Min(0f), Tooltip("Timer of the sprinting speed when making the step sound")] private float sprintStepMultiplier = 0.6f;

        //Sample of how an array of stepSounds should be declared (And then assigned in the inspector) for later iteration
        //[SerializeField] private AudioClip[] sampleClips = default;

        private float footstepTimer = 0f;
        private AudioSource footstepAudioSource = default;

        private float GetCurrentOffset => baseStepSpeed * (isCrouching ? crouchStepMultiplier : IsSprinting ? sprintStepMultiplier : 1f);


        //Sliding parameters
        private Vector3 hitPointNormal;
        private bool IsSliding
        {
            get
            {
                if(characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
                {
                    hitPointNormal = slopeHit.normal;
                    return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
                }
                else
                {
                    return false;
                }
            }
        }
        //End of Sliding parameters

        //Input system handles class
        private PlayerControlInputs playerInputs;        


        private void Awake()
        {
            //Getting references
            characterController = GetComponent<CharacterController>();
            playerInputs = GetComponent<PlayerControlInputs>();
            footstepAudioSource = GetComponent<AudioSource>();

            playerCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            if (!playerCamera)
            {
                GameObject go = new("playerCamera", typeof(CinemachineVirtualCamera));
                go.transform.parent = this.transform;
                playerCamera = go.GetComponent<CinemachineVirtualCamera>();
            }


        }

        private void Start()
        {
            //Assigning variables    
            defaultYPosition = playerCamera.transform.localPosition.y;
            defaultFOV = playerCamera.m_Lens.FieldOfView;

            currentHealth = maxHeatlh;
            currentStamina = maxStamina;
        }

        private void OnEnable()
        {
            //Subscribing to events
            playerInputs.OnZoomChanged += HandleZoom;
            playerInputs.OnInteractChanged += HandleInteractionInput;

            OnTakeDamage += ApplyDamage;
        }

        private void OnDisable()
        {
            //UnSubscribing to events
            playerInputs.OnZoomChanged -= HandleZoom;
            playerInputs.OnInteractChanged -= HandleInteractionInput;

            OnTakeDamage -= ApplyDamage;
        }

        private void Update()
        {
            if (CanMove)
            {
                HandleMovementInput();
                HandleMouseLook();

                if (canJump)
                    HandleJump();

                if (canCrouch)
                    HandleCrouch();

                if (headbobEnabled)
                    HandleHeadbob();

                if (canInteract)
                    HandleInteractionCheck();

                if(useFootsteps)
                    HandleFootsteps();

                if (useStaminaSystem)
                    HandleStamina();


                ApplyFinalMovements();
            }
        }

        /// <summary>
        /// Handles the movement Input and calculation based on the playerInputs class.
        /// </summary>
        private void HandleMovementInput()
        {
            currentInput = playerInputs.Move * (isCrouching ? crouchSpeed: IsSprinting ? sprintSpeed  : defaultSpeed);

            float moveDirectionY = moveDirection.y;
            moveDirection = (transform.TransformDirection(Vector3.right) * currentInput.x) + (transform.TransformDirection(Vector3.forward) * currentInput.y);
            moveDirection.y = moveDirectionY;
        }

        /// <summary>
        /// Handles the movement of the camera based on the playerInputs location of the mouse.
        /// </summary>
        private void HandleMouseLook()
        {
            xRotation -= playerInputs.Look.y * lookSpeedVerticalAxis;
            xRotation = Mathf.Clamp(xRotation, -upperLookLimitDegrees, lowerLookLimitDegrees);

            playerCamera.transform.localRotation = Quaternion.Euler(-xRotation, 0f, 0f);

            transform.rotation *= Quaternion.Euler(0, playerInputs.Look.x * lookSpeedHorizontalAxis, 0f);
        }

        /// <summary>
        /// Handles the jumping based on the jump force.
        /// </summary>
        private void HandleJump()
        {
            if (ShouldJump)
            { 
                moveDirection.y = jumpForce;
            }
        }

        /// <summary>
        /// Handles the crouch or standing.
        /// </summary>
        private void HandleCrouch()
        {
            if(ShouldCrouch)
            {
                StartCoroutine(CrouchStand());
            }
        }

        /// <summary>
        /// Handles the HeadBob movement, not applying if character is not grounded.
        /// </summary>
        private void HandleHeadbob()
        {
            if (!characterController.isGrounded) return;
            
            if(Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
            {
                headbobTimer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : defaultBobSpeed);
                playerCamera.transform.localPosition = new Vector3(
                    playerCamera.transform.localPosition.x,
                    defaultYPosition + Mathf.Sin(headbobTimer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : defaultBobAmount),
                    playerCamera.transform.localPosition.z);
            }
            else if (defaultYPosition != playerCamera.transform.localPosition.y)
            {
                headbobTimer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : defaultBobSpeed);
                playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, defaultYPosition, playerCamera.transform.localPosition.z);
            }
        }

        /// <summary>
        /// Handles the zooming in/out.
        /// </summary>
        /// <param name="isEntering">Whether or not isEntering to the zoom state (IE going from not-zoomed to zoomed)</param>
        private void HandleZoom(bool isEntering)
        {
            if(canZoom)
            {
                if (zoomRoutine != null)
                {
                    StopCoroutine(zoomRoutine);
                    zoomRoutine = null;
                }

                zoomRoutine = StartCoroutine(ToggleZoom(isEntering));
            }
        }

        /// <summary>
        /// Checks whether there's an interactable in view or not.
        /// </summary>
        private void HandleInteractionCheck()
        {
            if(Physics.Raycast(Camera.main.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
            {
                if (hit.collider.gameObject.layer == 6 && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.gameObject.GetInstanceID()))
                {
                    hit.collider.TryGetComponent(out currentInteractable);

                    if (currentInteractable)
                        currentInteractable.OnFocus();

                }
            }
            else if (currentInteractable)
            {
                currentInteractable.OnLoseFocus();
                currentInteractable = null;
            }
        }

        /// <summary>
        /// Checks if the button to be interacted was pressed when looking at an interactable object.
        /// </summary>
        /// <param name="wasPressed">Whether or not the designed button for interaction was pressed or not</param>
        private void HandleInteractionInput(bool wasPressed)
        {
            if( (canInteract && wasPressed) && currentInteractable != null && Physics.Raycast(Camera.main.ViewportPointToRay(interactionRayPoint), interactionDistance, interactionLayer))
            {
                currentInteractable.OnInteract();
            }
        }

        /// <summary>
        /// Handles the footsteps sound module, won't produce any sound if character is not grounded or isn't pressing movement keys.
        /// </summary>
        private void HandleFootsteps()
        {
            if (!characterController.isGrounded) return;

            if (currentInput == Vector2.zero) return;

            footstepTimer -= Time.deltaTime;
            
            if(footstepTimer <= 0)
            {
                if(Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3f))
                {
                    //Here we can use the previously declared sample clip 
                    switch(hit.collider.tag)
                    {
                        //case "<Tag defined for the collision material>":
                        //    footstepAudioSource.PlayOneShot(sampleClips[Random.Range(0, sampleClips.Length-1)]);
                        //    break;
                        default:
                            //Default sound for hitting a non matched case object, for now it's just a warning, feel free to delete it.
                            Debug.LogWarning("Using module of footstep sound, but no footstep sound case was matched. Make sure to add the variable and the case to the corresponding sound.");
                            break;
                    }
                }

                footstepTimer = GetCurrentOffset;
            }
        }

        /// <summary>
        /// Damage to apply to the health system of the controller.
        /// </summary>
        /// <param name="dmg">Damage applied, sign is indifferent</param>
        private void ApplyDamage(float dmg)
        {
            if(useHealthSystem)
            {
                dmg = Mathf.Abs(dmg);
                currentHealth -= dmg;
                OnDamage?.Invoke(currentHealth);

                if (currentHealth <= 0) KillPlayer();
                else if (regeneratingHealth != null) StopCoroutine(regeneratingHealth);

                regeneratingHealth = StartCoroutine(RegenerateHealth());
            }
            else
            {
                Debug.LogWarning("OnTakeDamage was called but the useHealthSystem bool variable is false. Nothing will happen and you shouldn't be calling the Action if not using the health system.");
            }
        }

        /// <summary>
        /// Kills the player when health reaches 0.
        /// </summary>
        private void KillPlayer()
        {
            currentHealth = 0f;
            if (regeneratingHealth != null) StopCoroutine(RegenerateHealth());

            Debug.Log("Player died");
        }

        /// <summary>
        /// Handles the stamina system
        /// </summary>
        private void HandleStamina()
        {
            if (IsSprinting && currentInput != Vector2.zero)
            {
                if (regeneratingStamina != null)
                {
                    StopCoroutine(regeneratingStamina);
                    regeneratingStamina = null;
                }

                currentStamina -= staminaUseMultiplier * Time.deltaTime;

                if (currentStamina < 0f)
                {
                    currentStamina = 0f;
                }

                OnStaminaChange?.Invoke(currentStamina);

                if (currentStamina <= 0f)
                {
                    canSprint = false;
                }
            }

            if (!IsSprinting && currentStamina < maxStamina && regeneratingStamina == null)
            {
                regeneratingStamina = StartCoroutine(RegenerateStamina());
            }
        }

        /// <summary>
        /// Applies the actual movement using the characterController component and the moveDirection variable.
        /// </summary>
        private void ApplyFinalMovements()
        {
            if (!characterController.isGrounded) moveDirection.y -= gravity * Time.deltaTime;
            if (characterController.velocity.y < - 1 && characterController.isGrounded) moveDirection.y = 0;

            if (willSlideOnSlopes && IsSliding)
                moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSlideSpeed;

            characterController.Move(moveDirection * Time.deltaTime);
        }

        /// <summary>
        /// The Coroutine in charge for the change in movement from standing to crouch or viceversa.
        /// </summary>
        /// <returns>IEnumerator Coroutine</returns>
        private IEnumerator CrouchStand()
        {
            if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
                yield break;


            duringCrouchAnimation = true;

            float timeElapsed = 0f;
            float targetHeight = isCrouching ? standingHeight : crouchingHeight;
            float currentHeight = characterController.height;

            Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
            Vector3 currentCenter = characterController.center;

            while(timeElapsed < timeToCrouch)
            {
                characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/timeToCrouch);
                characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/timeToCrouch);

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            characterController.height = targetHeight;
            characterController.center = targetCenter;

            isCrouching = !isCrouching;

            duringCrouchAnimation = false;
        }

        /// <summary>
        /// The Coroutine in charge for the change in zoom, used when zooming in or zooming out.
        /// </summary>
        /// <param name="isEntering">Whether or not is entering zoom mode or not</param>
        /// <returns>IEnumerator Coroutine</returns>
        private IEnumerator ToggleZoom(bool isEntering)
        {
            float targetFOV = isEntering ? zoomFOV : defaultFOV;
            float startingFOV = playerCamera.m_Lens.FieldOfView;
            float timeElapsed = 0f;

            while(timeElapsed < timeToZoom)
            {
                playerCamera.m_Lens.FieldOfView = Mathf.Lerp(startingFOV, targetFOV, timeElapsed/timeToZoom);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            playerCamera.m_Lens.FieldOfView = targetFOV;
            zoomRoutine = null;
        }

        /// <summary>
        /// The Coroutine in charge for the regeneration of the health system.
        /// </summary>
        /// <returns>IEnumerator Coroutine</returns>
        private IEnumerator RegenerateHealth()
        {
            yield return new WaitForSeconds(timeBeforeRegenStarts);
            WaitForSeconds timeToWait = new(healthTimeIncrement);

            while(currentHealth < maxHeatlh)
            {
                currentHealth += healthValueIncrement;

                if (currentHealth > maxHeatlh) currentHealth = maxHeatlh;

                OnHeal?.Invoke(currentHealth);
                yield return timeToWait;
            }

            regeneratingHealth = null;
        }

        /// <summary>
        /// The Coroutine in charge for the regeneration of the stamina system.
        /// </summary>
        /// <returns>IEnumerator Coroutine</returns>
        private IEnumerator RegenerateStamina()
        {
            yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
            WaitForSeconds timeToWait = new(staminaTimeIncrement);

            while (currentStamina < maxStamina)
            {
                if (currentStamina > 0)
                    canSprint = true;

                currentStamina += staminaValueIncrement;

                if (currentStamina > maxStamina)
                    currentStamina = maxStamina;

                OnStaminaChange?.Invoke(currentStamina);

                yield return timeToWait;
            }

            regeneratingStamina = null;
        }
    }
}
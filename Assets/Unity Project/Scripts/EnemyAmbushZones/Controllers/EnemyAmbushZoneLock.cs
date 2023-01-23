using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Listens for the Player and Cameras to be close to a certain point and activates
/// </summary>
[RequireComponent(typeof(EnemyAmbushZoneScript))]
public class EnemyAmbushZoneLock : MonoBehaviour
{
    [SerializeField]
    private bool m_PlayerInZone;

    public bool PlayerInZone { get => m_PlayerInZone; }

    [SerializeField]
    private bool m_LockIsActive = false;

    public bool LockIsActive { get => m_LockIsActive; }

    public const float LOCK_CAMERA_DISTANCE = 2f;

    private Bounds m_PreviousLevelBounds;

    private EnemyAmbushZoneScript m_AmbushZone;
    private GameObject m_PlayerGO;
    private GameObject m_CameraGO;
    private GameObject m_CameraStackGO;
    private Camera m_MainCamera;
    private CinemachineCameraController m_CinemachineCamera;
    private LevelManager m_LevelManager;

    private void Awake()
    {
        m_AmbushZone = GetComponent<EnemyAmbushZoneScript>();
        m_LevelManager = m_AmbushZone.LevelManager;
        m_PreviousLevelBounds = m_LevelManager.LevelBounds;
    }

    private void Start()
    {
        m_MainCamera = Camera.main;
        m_CameraGO = m_MainCamera.gameObject;
        m_CameraStackGO = m_MainCamera.transform.parent.gameObject;
        m_CinemachineCamera = m_CameraStackGO.GetComponentInChildren<CinemachineCameraController>();
    }

    // + + + + | Functions | + + + +

    /// <summary>
    /// Activates the Lock, restricting the level bounds and preventing player progression until the lock is deactivated.
    /// </summary>
    public void ActivateLock()
    {
        m_LockIsActive = true;

        // Calculate & Set LevelBounds
        Vector2 screenBounds = m_MainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 screenOrigin = m_MainCamera.ScreenToWorldPoint(Vector2.zero);
        m_PreviousLevelBounds = m_LevelManager.LevelBounds;
        m_LevelManager.LevelBounds = new(transform.position, new Vector3(Mathf.Abs(screenOrigin.x) + Mathf.Abs(screenBounds.x), Mathf.Abs(screenOrigin.y) + Mathf.Abs(screenBounds.y), m_PreviousLevelBounds.size.z));

        // Lock Camera
        m_CinemachineCamera.StopFollowing();

        // Position Cameras to center
        // TODO: Coroutine for this?
        m_CinemachineCamera.transform.position = new Vector3(transform.position.x, transform.position.y, m_CinemachineCamera.transform.position.z);
        m_CameraGO.transform.position = m_CinemachineCamera.transform.position;

        // Trigger EAZScript's functions.
        m_AmbushZone.OnLockActivated(); // TODO: Via event perhaps?
    }

    /// <summary>
    /// Deactivates the Lock, restoring the level bounds and allowing player progression.
    /// </summary>
    public void DeactivateLock()
    {
        if (m_PreviousLevelBounds == null)
        {
            Debug.LogError("EAZLock - m_PreviousLevelBounds cannot be null!");
        }
        else
        {
            m_LevelManager.LevelBounds = m_PreviousLevelBounds;
        }

        // Release Camera
        m_CinemachineCamera.StartFollowing();

        m_LockIsActive = false;
    }

    // + + + + | Collision Handling | + + + +

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"{collision.gameObject.name} entered!");

        if (collision.gameObject.CompareTag("Player"))
        {
            m_PlayerInZone = true;
            m_PlayerGO = collision.gameObject;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Only Update Relevant to Collision
        if (m_AmbushZone.IsCompleted) return; // If this zone has been beaten, we don't care.
        if (m_LockIsActive) return; // We don't care if the lock is active
        if (!m_PlayerInZone) return; // We don't care if the player isn't in the zone

        // Check for camera position
        Vector2 camPosition = new Vector2(m_CameraGO.transform.position.x, m_CameraGO.transform.position.y);
        Vector2 thisPosition = new Vector2(transform.position.x, transform.position.y);
        if (Vector2.Distance(camPosition, thisPosition) <= LOCK_CAMERA_DISTANCE)
        {
            ActivateLock();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_PlayerInZone = false;
            m_PlayerGO = null;
        }
    }
}
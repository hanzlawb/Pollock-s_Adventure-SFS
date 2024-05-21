using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/**
 * This script is attached to the Gun Model on the Character Prefab
 */

public class SF2X_AimIK : MonoBehaviour
{
    #region  Variables
    public Transform spine;
    private Camera cam;
    public bool isPlayer;
    public bool shoot;
    public GameObject muzzleFlash;
    public Transform barrelLocation;
    private GameObject flash;
    private Animator animator;
    private float destroyTimer = 0.1f;
    #endregion

    /**
    * The Camera is needed for the raycast for the aiming
    * The animator is required to obtain the character spine bone
    */
    private void Awake()
    {
        cam = Camera.main;
        animator = GetComponentInParent<Animator>();
        spine = animator.GetBoneTransform(HumanBodyBones.Spine).transform;
    }
    /**
     * The code to rotate the spine for aiming must be on a LateUpdate
     * This ensures that all animation have been completed prior to the spine rotate
     */

    #region  Unity Methods
    private void LateUpdate()
    {
        if (this.isPlayer)
        {
            Vector3 mainCamPos = cam.transform.position;
            Vector3 dir = cam.transform.forward;
            Ray ray = new Ray(mainCamPos, dir);
            spine.LookAt(ray.GetPoint(40), Vector3.up);
        }
    }

    /**
    * On Shoot, we instantiate a muzzle flash on the Barrel end
    * This can be adjusted in the character prefab, as can the duration of the effect.
    */
    private void Update()
    {
        if (shoot)
        {
            shoot = false;
            flash = Instantiate(muzzleFlash, barrelLocation.position, barrelLocation.rotation);
            Destroy(flash, destroyTimer);
        }
    }
    #endregion
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AfterImageTrail : MonoBehaviour
{
    [SerializeField] float trailCooldownTimer = 0.01f;
    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float runSpeed = 50f;
    [SerializeField] float afterImageDuration = 1f;
    [SerializeField] Material trailMaterial;

    public delegate GameObject GetPooledObject(); 

    public GetPooledObject GetPooledGameobject;

    InputActions inputs;

    Rigidbody rb;

    MeshRenderer[] allMesheRenderers;
    SkinnedMeshRenderer[] allSkinnedRenderers;
    MeshFilter[] filters;
    CombineInstance[] meshesToCombine;

    Vector2 moveDir;

    float trailActivatedTime, currentSpeed;
    bool isTrailActive = false;

    private void Awake()
    {
        inputs = new();
        rb = GetComponent<Rigidbody>();
        currentSpeed = moveSpeed;

        // get the meshs renderers and combine them
        allSkinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        allMesheRenderers = GetComponentsInChildren<MeshRenderer>();
        filters = new MeshFilter[allMesheRenderers.Length];
        for (int i = 0; allMesheRenderers.Length > i; i++)
        {
            filters[i] = allMesheRenderers[i].GetComponent<MeshFilter>();
        }

        meshesToCombine = new CombineInstance[allMesheRenderers.Length+ allSkinnedRenderers.Length];
        
    }

    private void OnEnable()
    {
        inputs.Enable();

        inputs.Player.Move.performed += OnMovementPreformed;
        inputs.Player.Move.canceled += OnMovementCanceled;
        inputs.Player.ActivateTrail.started += OnActivetedTrail;
    }
    private void OnDisable()
    {
        inputs.Player.Move.performed -= OnMovementPreformed;
        inputs.Player.Move.canceled -= OnMovementCanceled;
        inputs.Player.ActivateTrail.started -= OnActivetedTrail;

        inputs.Disable();
    }

    private void Update()
    {
        // if the trail is active the player is moving and the cooldown is ready then create it 
        if (isTrailActive && rb.linearVelocity != Vector3.zero && Time.time - trailActivatedTime >= trailCooldownTimer)
        {
            trailActivatedTime = Time.time;
            CreateTrail();
        }
    }
    private void FixedUpdate()
    {
        Vector3 dir = new(moveDir.x, 0, moveDir.y);
        rb.linearVelocity = dir * currentSpeed ;

        if (rb.linearVelocity == Vector3.zero)
            return;

        Quaternion rot = Quaternion.LookRotation(rb.linearVelocity,Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation,rot,20);
    }

    private void OnActivetedTrail(InputAction.CallbackContext context)
    {
        isTrailActive = !isTrailActive;
        trailActivatedTime = Time.time;

        if (isTrailActive)
            currentSpeed = runSpeed;
        else
            currentSpeed = moveSpeed;
    }

    private void OnMovementPreformed(InputAction.CallbackContext context)
    {
        // movement calculation
        moveDir = context.ReadValue<Vector2>();
    }
    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        moveDir = Vector2.zero;
    }

    void CreateTrail()
    {
        Matrix4x4 matrix = transform.worldToLocalMatrix;

        // insert the skinned meshes to combine in the array
        for (int i = 0; i < allSkinnedRenderers.Length; i++) 
        {
            var skin = allSkinnedRenderers[i];

            Mesh bakeMesh = new();
            skin.BakeMesh(bakeMesh);
            meshesToCombine[i].mesh = bakeMesh;
            meshesToCombine[i].transform = matrix * skin.localToWorldMatrix ;
        }

        // insert the meshes to combine in the array
        for (int i = 0; i < allMesheRenderers.Length; i++)
        {
            var renderer = allMesheRenderers[i];
            var currentFilter = filters[i];

            i = allSkinnedRenderers.Length + i;

            meshesToCombine[i].mesh = currentFilter.sharedMesh;
            meshesToCombine[i].transform = matrix * renderer.localToWorldMatrix ;
        }

        // create the gameobject and move it to the correct position
        GameObject obj = GetPooledGameobject?.Invoke();
        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;

        // crate a MeshRenderer and give it to the obj
        Renderer objRenderer = obj.GetComponent<MeshRenderer>();
        objRenderer.material = trailMaterial;

        

        // create a MeshFilter and give it to the obj
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        filter.mesh.CombineMeshes(meshesToCombine);


        // destroy after x seconds
        StartCoroutine(DestroyGameobject(obj,objRenderer));

    }

    private IEnumerator DestroyGameobject(GameObject obj, Renderer objRen)
    {
        float timer = afterImageDuration;
        
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            objRen.material.SetFloat("_AlphaLerped", Mathf.Lerp(0, 1, timer / afterImageDuration));
            yield return null;
        }
        
        obj.SetActive(false);
    }
}

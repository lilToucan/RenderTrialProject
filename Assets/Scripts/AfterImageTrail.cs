using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AfterImageTrail : MonoBehaviour
{
    [SerializeField] float trailCooldownTimer = 0.01f;
    [SerializeField] float moveSpeed = 600f;
    [SerializeField] Material trailMaterial;

    InputActions inputs;

    Rigidbody rb;

    MeshRenderer[] allMesheRenderers;
    SkinnedMeshRenderer[] allSkinnedRenderers;
    MeshFilter[] filters;
    CombineInstance[] meshesToCombine;

    Vector2 moveDir;

    float trailActivatedTime;
    bool isTrailActive = false;

    private void Awake()
    {
        inputs = new();
        rb = GetComponent<Rigidbody>();


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
        rb.linearVelocity = dir * moveSpeed ;
    }

    private void OnActivetedTrail(InputAction.CallbackContext context)
    {
        isTrailActive = !isTrailActive;
        trailActivatedTime = Time.time;
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
            meshesToCombine[i].transform = skin.localToWorldMatrix * matrix;
        }

        // insert the meshes to combine in the array
        for (int i = allSkinnedRenderers.Length; meshesToCombine.Length > i; i++)
        {
            var renderer = allMesheRenderers[i];

            meshesToCombine[i].mesh = filters[i].sharedMesh;
            meshesToCombine[i].transform = renderer.localToWorldMatrix * matrix;
        }

        // create the gameobject and move it to the correct position
        GameObject obj = new GameObject("obj number: ");
        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;

        // crate a MeshRenderer and give it to the obj
        MeshRenderer objRenderer = obj.AddComponent<MeshRenderer>();
        objRenderer.material = trailMaterial;

        // !!! change the material propreties here:

        // create a MeshFilter and give it to the obj
        MeshFilter filter = obj.AddComponent<MeshFilter>();
        filter.mesh.CombineMeshes(meshesToCombine);


        // destroy after x seconds
        StartCoroutine(DestroyGameobject(obj));

    }

    private IEnumerator DestroyGameobject(GameObject obj)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(obj);
    }
}

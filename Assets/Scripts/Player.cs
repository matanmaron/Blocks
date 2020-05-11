using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float gravity = -9.8f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float playerHeight = 1.62f;
    public float playerWidth = 0.15f;
    public bool isGrounded;
    public bool isSprinting;
    public Transform highlightBlock;
    public Transform placeHighlightBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8;
    public TextMeshProUGUI SelectedBlockTMP;
    public byte selectedBlockIndex = 1;
    Transform cam;
    World world;
    float horizontal;
    float vertical;
    float mouseHorizontal;
    float mouseVertical;
    float verticalMomentum = 0;
    bool jumpRequet;
    Vector3 Velocity;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
        Cursor.lockState = CursorLockMode.Locked;
        SelectedBlockTMP.text = world.blockTypes[selectedBlockIndex].blockName;
    }
    private void FixedUpdate()
    {
        CalculateVelocity();
        if (jumpRequet)
        {
            Jump();
        }
        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * -mouseVertical);
        transform.Translate(Velocity, Space.World);
    }

    private void Update()
    {
        GetPlayerInputs();
        PlaceCursorBlocks();
    }

    private void CalculateVelocity()
    {
        if (verticalMomentum > gravity) // gravity
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }
        if (isSprinting) // sprint
        {
            Velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        }
        else
        {
            Velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        }
        // fall/jump
        Velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((Velocity.z > 0 && front) || (Velocity.z < 0 && back))
        {
            Velocity.z = 0;
        }
        if ((Velocity.x > 0 && right) || (Velocity.x < 0 && left))
        {
            Velocity.x = 0;
        }
        if (Velocity.y < 0)
        {
            Velocity.y = CheckDownSpeed(Velocity.y);
        }
        else if (Velocity.y > 0)
        {
            Velocity.y = CheckUpSpeed(Velocity.y);
        }
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequet = false;
    }

    void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Fire3")) //sprint
        {
            isSprinting = true;
        }
        if (Input.GetButtonUp("Fire3"))
        {
            isSprinting = false;
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequet = true;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0) //up
            {
                selectedBlockIndex++;
            }
            else //down
            {
                selectedBlockIndex--;
            }
            if (selectedBlockIndex > (byte)world.blockTypes.Length - 1)
            {
                selectedBlockIndex = 1;
            }
            if (selectedBlockIndex < 1)
            {
                selectedBlockIndex = (byte)(world.blockTypes.Length - 1);
            }
            SelectedBlockTMP.text = world.blockTypes[selectedBlockIndex].blockName;
        }

        if (highlightBlock.gameObject.activeSelf)
        {
            //destroy
            if (Input.GetMouseButtonDown(0))
            {
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
            }
            //build
            if (Input.GetMouseButtonDown(1))
            {
                world.GetChunkFromVector3(placeHighlightBlock.position).EditVoxel(placeHighlightBlock.position, selectedBlockIndex);
            }
        }
    }

    void PlaceCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();
        while(step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);
            if (world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeHighlightBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeHighlightBlock.gameObject.SetActive(true);

                return;
            }
            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }
        highlightBlock.gameObject.SetActive(false);
        placeHighlightBlock.gameObject.SetActive(false);
    }

    float CheckDownSpeed(float downSpeed)
    {
        if (
            world.CheckForVoxel(transform.position.x - playerWidth,transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth,transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth,transform.position.y + downSpeed, transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth,transform.position.y + downSpeed, transform.position.z + playerWidth)
            )
        {
            isGrounded = true;
            return 0;
        }

        isGrounded = false;
        return downSpeed;
    }
    float CheckUpSpeed(float upSpeed)
    {
        if (
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + upSpeed + 2f, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + upSpeed + 2f, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + upSpeed + 2f, transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + upSpeed + 2f, transform.position.z + playerWidth)
            )
        {
            return 0;
        }

        return upSpeed;
    }

    public bool front
    {
        get
        {
            if (
                world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z + playerWidth) ||
                world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth)
                )
            {
                return true;
            }
            return false;
        }
    }
    public bool back
    {
        get
        {
            if (
                world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z - playerWidth) ||
                world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth)
                )
            {
                return true;
            }
            return false;
        }
    }
    public bool left
    {
        get
        {
            if (
                world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y, transform.position.z) ||
                world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z)
                )
            {
                return true;
            }
            return false;
        }
    }
    public bool right
    {
        get
        {
            if (
                world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y, transform.position.z) ||
                world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z)
                )
            {
                return true;
            }
            return false;
        }
    }
}

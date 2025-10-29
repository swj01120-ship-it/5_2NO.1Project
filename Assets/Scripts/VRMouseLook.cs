using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMouseLook : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 100f;       // ���콺 �ΰ���
    public Transform playerBody;                // �÷��̾� ���� (Y�� ȸ����)

    private float xRotation = 0f;               // ���� ȸ�� ��

    void Start()
    {
        // ���콺 Ŀ�� ����� �� ����
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ���콺 �Է� �ޱ�
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ���� ȸ�� (ī�޶�)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 90�� ����

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // �¿� ȸ�� (�÷��̾� ����)
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }

        // ESC Ű�� ���콺 ����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // ���콺 Ŭ������ �ٽ� ���
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

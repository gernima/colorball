using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Transform ball;
    private Vector3 startMousePos, startBallPos;
    private bool moveTheBall;
    [Range(0f, 1f)] public float maxSpeed;
    [Range(0f, 1f)] public float camSpeed;
    [Range(0f, 50f)] public float pathSpeed;
    [Range(0f, 1000f)] public float BallRotationSpeed;
    private float velocity, camVelocity_x, camVelocity_y;
    private Camera mainCamera;
    public Transform path;
    private Rigidbody rb;
    private Collider _collider;
    private Renderer BallRenderer;
    public ParticleSystem CollideParticle;
    public ParticleSystem AirParticle;
    public ParticleSystem DustParticle;
    public ParticleSystem BallTrail;
    public Material[] BallMaterials = new Material[2];
    private void Start()
    {
        ball = transform;
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        BallRenderer = ball.GetChild(1).GetComponent<Renderer>();
        var BallTrail = this.BallTrail.trails;
        BallTrail.colorOverLifetime = BallMaterials[1].color;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Menu.instance.GameState)
        {
            moveTheBall = true;
            BallTrail.Play();
            Plane newPlane = new Plane(Vector3.up, 0f);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (newPlane.Raycast(ray, out var distance))
            {
                startMousePos = ray.GetPoint(distance);
                startBallPos = ball.position;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            moveTheBall = false;
        }

        if (moveTheBall)
        {
            Plane newPlane = new Plane(Vector3.up, 0f);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (newPlane.Raycast(ray, out var distance))
            {
                Vector3 mouseNewPos = ray.GetPoint(distance);
                Vector3 MouseNewPos = mouseNewPos - startMousePos;
                Vector3 DesireBallPos = MouseNewPos + startBallPos;

                DesireBallPos.x = Mathf.Clamp(DesireBallPos.x, -1.5f, 1.5f);

                ball.position = new Vector3(Mathf.SmoothDamp(ball.position.x, DesireBallPos.x, ref velocity, maxSpeed), ball.position.y, ball.position.z);
            }
        }
        if (Menu.instance.GameState)
        {
            var pathNewPos = path.position;

            path.position = new Vector3(pathNewPos.x, pathNewPos.y, Mathf.MoveTowards(pathNewPos.z, -1000000f, pathSpeed * Time.deltaTime));

            ball.GetChild(1).Rotate(Vector3.right * BallRotationSpeed * Time.deltaTime); 
        }
    }
    private void LateUpdate()
    {
        var CameraNewPos = mainCamera.transform.position;
        if (rb.isKinematic)
        {
            mainCamera.transform.position = new Vector3(Mathf.SmoothDamp(CameraNewPos.x, ball.position.x, ref camVelocity_x, camSpeed), 
                Mathf.SmoothDamp(CameraNewPos.y, ball.position.y + 3f, ref camVelocity_y, camSpeed), CameraNewPos.z);
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            gameObject.SetActive(false);
            Menu.instance.GameState = false;
            Menu.instance.menuElement[2].SetActive(true);
            Menu.instance.menuElement[2].transform.GetChild(0).GetComponent<Text>().text = "You Lose";
        }
        if (other.CompareTag("red") || other.CompareTag("green") || other.CompareTag("yellow") || other.CompareTag("blue"))
        {
            other.gameObject.SetActive(false);
            BallMaterials[1] = other.GetComponent<Renderer>().material;
            BallRenderer.materials = BallMaterials;
            var NewParticle = Instantiate(CollideParticle, transform.position, Quaternion.identity);
            NewParticle.GetComponent<Renderer>().material = BallMaterials[1];
            var BallTrail = this.BallTrail.trails;
            BallTrail.colorOverLifetime = BallMaterials[1].color;
        }
        if (other.gameObject.name.Contains("ColorBall"))
        {
            PlayerPrefs.SetInt("Score", PlayerPrefs.GetInt("Score")+1);
            Menu.instance.menuElement[1].GetComponent<Text>().text = PlayerPrefs.GetInt("Score").ToString();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("path"))
        {
            rb.isKinematic = _collider.isTrigger = false;
            rb.velocity = new Vector3(0f, 8f, 0f);
            pathSpeed *= 2;
            BallRotationSpeed *= 2;
            var airEffectMain = AirParticle.main;
            airEffectMain.simulationSpeed = 10f;
            BallTrail.Stop();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("path"))
        {
            rb.isKinematic = _collider.isTrigger = true;
            pathSpeed /= 2;
            BallRotationSpeed /= 2;
            var airEffectMain = AirParticle.main;
            airEffectMain.simulationSpeed = 4f;

            DustParticle.transform.position = collision.contacts[0].point + new Vector3(0f, 0.3f, 0f);
            DustParticle.GetComponent<Renderer>().material = BallMaterials[1];
            DustParticle.Play();
            BallTrail.Play();
        }
    }
}

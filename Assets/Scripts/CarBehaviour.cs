using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehaviour : MonoBehaviour {

    public bool thrustEnabled = false;
    
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public float maxTorque = 1000;
    public float lowSpeedSteerAngel = 10;
    public float highSpeedSteerAngel = 1;
    public float maxSteerAngle = 45;
    public Transform centerOfMass;
    private Rigidbody _rigidbody;
    private float _currentSpeedKMH;
    public float maxSpeedKMH = 120f;
    public float maxSpeedBackwardKMH = 30f;
    public float maxBrakeTorque = 5000;
    private float _brakeTorque;
    private float _motorTorque;
    public GUIText guiSpeed;
    public GUIText guiTime;
    public Texture2D guiSpeedDisplay;
    public Texture2D guiSpeedPointer;
    public AudioClip engineSingleRPMSoundClip;
    private AudioSource _engineAudioSource;
    private float degPerKMH;
    private object engineSoundSource;
    private bool velocityIsForeward;
    private bool doBraking;
    public ParticleSystem smokeL;
    public ParticleSystem smokeR;
    private ParticleSystem.EmissionModule _smokeLEmission;
    private ParticleSystem.EmissionModule _smokeREmission;
    private ParticleSystem _dustFLEmission;
    private ParticleSystem _dustFREmission;
    private ParticleSystem _dustRLEmission;
    private ParticleSystem _dustRREmission;


    // Use this for initialization
    void Start() {

        _rigidbody = GetComponent<Rigidbody>();
        _currentSpeedKMH = 0;

        _rigidbody.centerOfMass = new Vector3(centerOfMass.localPosition.x,
                                             centerOfMass.localPosition.y,
                                             centerOfMass.localPosition.z);
   
        guiSpeed.color = Color.white;

        _engineAudioSource = gameObject.AddComponent<AudioSource>();
        _engineAudioSource.clip = engineSingleRPMSoundClip;
        _engineAudioSource.loop = true;
        _engineAudioSource.volume = 0.7f;
        _engineAudioSource.playOnAwake = true;

          _smokeLEmission = smokeL.emission;
          _smokeREmission = smokeR.emission;
          _smokeLEmission.enabled = true;
          _smokeREmission.enabled = true;

    }


    private void Update()
    {

    }

    void FixedUpdate()
    {
    guiSpeed.text = _currentSpeedKMH.ToString("0") + " KMH";

            _currentSpeedKMH = _rigidbody.velocity.magnitude * 3.6f;

        velocityIsForeward = Vector3.Angle(transform.forward, _rigidbody.velocity) < 50f;

            if (velocityIsForeward)
            {
                if (_currentSpeedKMH < maxSpeedKMH)
                {
                    wheelFL.motorTorque = maxTorque * Input.GetAxis("Vertical");
                    wheelFR.motorTorque = wheelFL.motorTorque;
                }
                else
                {
                    wheelFL.motorTorque = 0;
                    wheelFR.motorTorque = 0;
                    _rigidbody.velocity = (maxSpeedKMH / 3.6f) * _rigidbody.velocity.normalized;
                }
            }
            else
            {
                if (_currentSpeedKMH < maxSpeedBackwardKMH)
                {
                    wheelFL.motorTorque = maxTorque * Input.GetAxis("Vertical");
                    wheelFR.motorTorque = wheelFL.motorTorque;
                }
                else
                {
                    wheelFL.motorTorque = 0;
                    wheelFR.motorTorque = 0;
                    _rigidbody.velocity = (maxSpeedBackwardKMH / 3.6f) * _rigidbody.velocity.normalized;
                }
            }

       

{
                SetMotorTorque(maxTorque * Input.GetAxis("Vertical"));
                SetSteerAngle(maxSteerAngle * Input.GetAxis("Horizontal"));
            }

            doBraking = _currentSpeedKMH > 0.5f &&
                (Input.GetAxis("Vertical") < 0 && velocityIsForeward ||
                 Input.GetAxis("Vertical") > 0 && !velocityIsForeward);

            if (doBraking)
            { wheelFL.brakeTorque = maxBrakeTorque;
                wheelFR.brakeTorque = maxBrakeTorque;
                wheelRL.brakeTorque = maxBrakeTorque;
                wheelRR.brakeTorque = maxBrakeTorque;
                wheelFL.brakeTorque = 0;
                wheelFR.brakeTorque = 0;
            } else
            {
                wheelFL.brakeTorque = 0;
                wheelFR.brakeTorque = 0;
                wheelRL.brakeTorque = 0;
                wheelRR.brakeTorque = 0;
                wheelFL.motorTorque = maxTorque * Input.GetAxis("Vertical");
                wheelFR.motorTorque = wheelFL.motorTorque;
            }

           float speedFactor = _rigidbody.velocity.magnitude / maxSpeedKMH;
           var currentSteerAngel = Mathf.Lerp(lowSpeedSteerAngel, highSpeedSteerAngel, speedFactor);
           currentSteerAngel *= Input.GetAxis("Horizontal");
           wheelFL.steerAngle = currentSteerAngel;
           wheelFR.steerAngle = currentSteerAngel;

        wheelFL.attachedRigidbody.AddForce(-transform.up * 100 * wheelFL.attachedRigidbody.velocity.magnitude);

        wheelFR.attachedRigidbody.AddForce(-transform.up * 100 * wheelFR.attachedRigidbody.velocity.magnitude);

        float steerFactor = Mathf.Max(-10f / maxSpeedKMH * _currentSpeedKMH + 20f);
        wheelFL.steerAngle = steerFactor * Input.GetAxis("Horizontal");
        wheelFR.steerAngle = wheelFL.steerAngle;

        int gearNum = 0;
        float engineRPM = kmh2rpm(_currentSpeedKMH, out gearNum);
        SetEngineSound(engineRPM);

        SetParticleSystems(engineRPM);


    }

        void SetMotorTorque(float amount)
    {
            wheelFL.motorTorque = amount;
            wheelFR.motorTorque = amount;
        }

        void SetSteerAngle(float angle)
    {
            wheelFL.steerAngle = angle;
            wheelFR.steerAngle = angle;
        }


    void SetFriction(float forewardFriction, float sidewaysFriction)

    {
        WheelFrictionCurve f_fwWFC = wheelFL.forwardFriction;
        WheelFrictionCurve f_swWFC = wheelFL.sidewaysFriction;

        f_fwWFC.stiffness = forewardFriction;
        f_swWFC.stiffness = sidewaysFriction;

        wheelFL.forwardFriction = f_fwWFC;
        wheelFL.sidewaysFriction = f_swWFC;
        wheelFR.forwardFriction = f_fwWFC;
        wheelFR.sidewaysFriction = f_swWFC;
        wheelRL.forwardFriction = f_fwWFC;
        wheelRL.sidewaysFriction = f_swWFC;
        wheelRR.forwardFriction = f_fwWFC;
        wheelRR.sidewaysFriction = f_swWFC;

       
    }



void OnGUI()
    {
        // Scale everything to the screen height.
        float scale = 3.0f;
        int sh = Screen.height;
        int size = (int)(sh / scale); // size of speed meter

        int lenN = (int)(size * 0.7777f); // length of needle
        int offN = (int)(size / 8.2f); // offset of needle
                                       // Draw speed meter
        GUI.DrawTexture(new Rect(0, sh - size, size, size),
        guiSpeedDisplay,
        ScaleMode.StretchToFill);
        // Rotate the the coordinate system around a point
        float degPerKMH = 2.057f;
        GUIUtility.RotateAroundPivot(Mathf.Abs(_currentSpeedKMH) * degPerKMH + 36,
        new Vector2(lenN / 2 + offN, sh - size +
        lenN / 2 + offN));
        // Draw the speed pointer
        GUI.DrawTexture(new Rect(offN, sh - size + offN, lenN, lenN),
        guiSpeedPointer,
        ScaleMode.StretchToFill);
    }

    class gear
    {
        public gear(float minKMH, float minRPM, float maxKMH, float maxRPM)
        {
            _minRPM = minRPM;
            _minKMH = minKMH;
            _maxRPM = maxRPM;
            _maxKMH = maxKMH;
        }
        private float _minRPM;
        private float _minKMH;
        private float _maxRPM;
        private float _maxKMH;
        public bool speedFits(float kmh)
        {
            return kmh >= _minKMH && kmh <= _maxKMH;
        }
        public float interpolate(float kmh)
        {
            return 50;
        }
    }

    float kmh2rpm(float kmh, out int gearNum)
    {
        gear[] gears = new gear[]
{ new gear( 1, 900, 12, 1400),
new gear( 12, 900, 25, 2000),
new gear( 25, 1350, 45, 2500),
new gear( 45, 1950, 70, 3500),
new gear( 70, 2500, 112, 4000),
new gear(112, 3100, 180, 5000)
};
        for (int i = 0; i < gears.Length; ++i)
        {
            if (gears[i].speedFits(kmh))
            {
                gearNum = i + 1;
                return gears[i].interpolate(kmh);
            }
        }
        gearNum = 1;
        return 800;
    }

    void SetEngineSound(float engineRPM)
    {
        if (engineSoundSource == null) return;
        float minRPM = 800;
        float maxRPM = 8000;
        float minPitch = 0.3f;
        float maxPitch = 3.0f;
        float pitch = 1.0f;
        _engineAudioSource.pitch = pitch;
    }

    void SetParticleSystems(float engineRPM)
    {
        float smokeRate = engineRPM / 10.0f;
        _smokeLEmission.rate = new ParticleSystem.MinMaxCurve(smokeRate);
        _smokeREmission.rate = new ParticleSystem.MinMaxCurve(smokeRate);

    }

    WheelHit GetGroundInfos(ref WheelCollider wheelCol,
ref string groundTag,
ref int groundTextureIndex)
    { // Default values
        groundTag = "InTheAir";
        groundTextureIndex = -1;
        WheelHit wheelHit;
        wheelCol.GetGroundHit(out wheelHit);
        // If not in the air query collider
        if (wheelHit.collider)
        {
            groundTag = wheelHit.collider.tag;
            if (wheelHit.collider.CompareTag("Terrain"))
                groundTextureIndex = TerrainSurface.GetMainTexture(transform.position);
        }
        return wheelHit;
    }
}





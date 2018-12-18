using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class Instanciate : MonoBehaviour {

    public Transform cube;
    public Transform current = null;
    public bool gravity = true;
    public bool isGlobal = false;
    public float maxTorque;
    public float maxForce;
    public int randomRadius;
    public int randomSize;

    public enum Time { Start, AcidStart, KickStart, Break, Drop, EndBreak };
    public Time currentTime;

    [FMODUnity.EventRef]
    public string PlayerStateEvent;
    FMOD.Studio.EventInstance playerState;
    FMOD.System lowlevelSystem;
    FMOD.DSP masterDSP;
    public List<FMOD.DSP> dsps;
    Leap.Controller leapControl;

    public static Instanciate singleton;

    float map(float val, float min, float high, float nMin, float nHigh)
    {
        float normal = Mathf.InverseLerp(min, high, val);
        return Mathf.Lerp(nMin, nHigh, normal);
    }

    void Start()
    {
        dsps = new List<FMOD.DSP>();
        singleton = this;
        playerState = FMODUnity.RuntimeManager.CreateInstance(PlayerStateEvent);
        playerState.setTimelinePosition(100000);

        lowlevelSystem = FMODUnity.RuntimeManager.LowlevelSystem;
        if (!masterDSP.hasHandle())
        {
            FMOD.ChannelGroup master;
            lowlevelSystem.getMasterChannelGroup(out master);
            master.getDSP(0, out masterDSP);
            masterDSP.setMeteringEnabled(false, true);
        }
        setupLeapMotion();
        currentTime = Time.Start;
    }

    public void setupLeapMotion()
    {
        leapControl = new Leap.Controller();
        HandHandler h = new HandHandler();
        leapControl.Connect += h.OnServiceConnect;
        leapControl.Device += h.OnConnect;
        leapControl.FrameReady += h.OnFrame;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            if (current != null)
            {
                current.transform.parent = null;
                current.GetComponent<Rigidbody>().isKinematic = false;
            }
            this.startTrack();
            current = Instantiate(cube, Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 3)), transform.rotation);
            current.transform.parent = this.transform.parent.gameObject.transform;
            current.GetComponent<Rigidbody>().isKinematic = true;
        }
        if (Input.GetKey("a"))
        {
            CubeData c = current.GetComponent<CubeData>();
            c.increase();
        }
        if (Input.GetKeyUp("a"))
        {
            current.transform.parent = null;
            current.GetComponent<Rigidbody>().isKinematic = false;

        }
        if (Input.GetKeyDown("m"))
        {
            toggleGravity();
        }
        if (Input.GetKeyDown("l"))
        {
            toggleGlobal();
        }
        if (Input.GetKey("b"))
        {
            GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");
            foreach (GameObject l in lights)
            {
                CubeData c = l.GetComponent<CubeData>();
                c.increase();
            }
        }
        if (Input.GetKey("n"))
        {
            GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");
            foreach (GameObject l in lights)
            {

                CubeData c = l.GetComponent<CubeData>();
                c.decrease();
            }
        }
        if (Input.GetKey("k"))
        {
            CubeData.globalColor = CubeData.hueRotate(CubeData.globalColor, 0.003f);
            GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");
            foreach (GameObject l in lights)
            {
                CubeData c = l.GetComponent<CubeData>();
                c.hueRotate();
            }
        }
        if (Input.GetKeyDown("v"))
        {
            ScreenCapture.CaptureScreenshot(System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");
        }
        if (dsps.Count > 0)
        {
            addRandomCube();
        }
        checkTimeChange();
    }

    private void getAllChannelDsp()
    {
        FMOD.ChannelGroup group;
        playerState.getChannelGroup(out group);
        int numGroupChannels, ignore;
        group.getNumGroups(out numGroupChannels);
        Debug.Log(numGroupChannels);
        for (int i = 0; i < numGroupChannels; i++)
        {
            int numDSPs;
            FMOD.ChannelGroup subgroup;
            group.getGroup(i, out subgroup);
            subgroup.getNumDSPs(out numDSPs);
            //Debug.Log("dsp : " + numDSPs);
            FMOD.DSP dsp;
            subgroup.getDSP(0, out dsp);
            dsp.setMeteringEnabled(false, true);
            dsps.Add(dsp);
        }
    }

    private void startTrack()
    {
        FMOD.Studio.PLAYBACK_STATE state;
        playerState.getPlaybackState(out state);
        if (state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            FMOD.RESULT res = playerState.start();
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(playerState, GetComponent<Transform>(), GetComponent<Rigidbody>());
            getAllChannelDsp();
        }
    }

    public float customMap(int index, float db)
    {
        if (index < 8)
        {
            return map(db, -55f, -15f, 0, 1);
        }
        else
        {
            return map(db, -40f, -10f, 0, 1);
        }
    }

    private Time getEventTimeline()
    {
        int time;
        playerState.getTimelinePosition(out time);
        if (time < 65000)
        {
            return Time.Start;
        } else if (time < 113000)
        {
            return Time.AcidStart;
        } else if (time < 211000)
        {
            return Time.KickStart;
        } else if (time < 227000)
        {
            return Time.Break;
        } else if (time < 390000)
        {
            return Time.Drop;
        } else
        {
            return Time.EndBreak;
        }
    }

    public void checkTimeChange()
    {
        Time newTime = getEventTimeline();
        if (newTime != currentTime)
        {
            timeChanged(newTime);
        }
    }

    public void timeChanged(Time t)
    {
        Debug.Log("time change");
        currentTime = t;
        StartCoroutine("hueRotate");
    }

    public void toggleGravity()
    {
        if (gravity)
        {
            gravityOff();
        }
        else
        {
            gravityOn();
        }
    }

    public void gravityOn()
    {
        gravity = true;
        Physics.gravity = new Vector3(0, -4.905F, 0);
    }

    public void gravityOff()
    {
        gravity = false;
        Physics.gravity = Vector2.zero;
        GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");
        foreach (GameObject l in lights)
        {
            CubeData c = l.GetComponent<CubeData>();
            c.addRandomForce();
        }
    }

    IEnumerator hueRotate()
    {
        DateTime start = System.DateTime.Now;
        DateTime elapsed = System.DateTime.Now;
        while (elapsed.Subtract(start).TotalSeconds < 8)
        {
            addRandomCube();
            GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");
            CubeData.globalColor = CubeData.hueRotate(CubeData.globalColor, 0.003f);
            foreach (GameObject l in lights)
            {
                CubeData c = l.GetComponent<CubeData>();
                c.hueRotate(0.003f);
            }
            elapsed = System.DateTime.Now;
            yield return null;
        }
    }

    public float getRmsLevel(int index)
    {
        FMOD.DSP dsp = dsps[index];
        FMOD.DSP_METERING_INFO outputMetering;
        dsp.getMeteringInfo(IntPtr.Zero, out outputMetering);
        float rms = 0;
        for (int i = 0; i < outputMetering.numchannels; i++)
        {
            rms += outputMetering.rmslevel[i] * outputMetering.rmslevel[i];
        }
        rms = Mathf.Sqrt(rms / (float)outputMetering.numchannels);

        float db = rms > 0 ? 20.0f * Mathf.Log10(rms * Mathf.Sqrt(2.0f)) : -80.0f;
        if (db > 10.0f) db = 10.0f;
        return customMap(index, db);
    }

    public float getGlobalRmsLevel()
    {
        FMOD.DSP_METERING_INFO outputMetering;
        masterDSP.getMeteringInfo(IntPtr.Zero, out outputMetering);
        float rms = 0;
        for (int i = 0; i < outputMetering.numchannels; i++)
        {
            rms += outputMetering.rmslevel[i] * outputMetering.rmslevel[i];
        }
        rms = Mathf.Sqrt(rms / (float)outputMetering.numchannels);

        float db = rms > 0 ? 20.0f * Mathf.Log10(rms * Mathf.Sqrt(2.0f)) : -80.0f;
        if (db > 10.0f) db = 10.0f;
        return map(db, -40f, -15f, 0, 1);
    }

    public void addRandomCube()
    {
        if (Random.Range(0,200) > 1)
        {
            return;
        }
        double x, z, angle;
        angle = Random.Range(0.0f, 2f*(float)Math.PI);
        x = Math.Cos(angle);
        z = Math.Sin(angle);
        Vector3 pos = new Vector3((float)x, 0f, (float)z) * Random.Range(15, 15 + randomRadius) + transform.position;
        pos.y = 15;

        Debug.Log("randomCube" + pos);
        Transform randomC = Instantiate(cube, pos , Random.rotation);
        randomC.GetComponent<CubeData>().addRandomTorque();
        for (int i = 0; i< Random.Range(10,randomSize); i++)
        {
            randomC.GetComponent<CubeData>().increase();
        }
    }

    public void toggleGlobal()
    {
        isGlobal = !isGlobal;
    }

    public void pinch(float pinch)
    {
        toggleGlobal();
    }
 
    private void IncreaseTerrain()
    {
        var terrainData = Terrain.activeTerrain.terrainData;

        // The Splat map (Textures)
        SplatPrototype[] splatPrototype = new SplatPrototype[terrainData.splatPrototypes.Length];
        for (int i = 0; i < terrainData.splatPrototypes.Length; i++)
        {
            splatPrototype[i] = terrainData.splatPrototypes[i];
            splatPrototype[i].tileOffset += new Vector2(0.1f,0.1f);
        }
        terrainData.splatPrototypes = splatPrototype;
    }
}

using System;
using UnityEngine;
using Leap;

public class HandHandler {
    Vector handPosition;
    DateTime timeSinceLastCall;

    public void OnServiceConnect(object sender, ConnectionEventArgs args)
    {
        Debug.Log("Service Connected");
    }

    public void OnConnect(object sender, DeviceEventArgs args)
    {
        Debug.Log("Connected");
        timeSinceLastCall = System.DateTime.Now;
    }

    public void OnFrame(object sender, FrameEventArgs args)
    {
        if ((System.DateTime.Now - timeSinceLastCall).Seconds < 1)
        {
            return;
        }
        Frame frame = args.frame;
        foreach (Hand hand in frame.Hands)
        {
            Vector newPosition = hand.PalmPosition;

            Vector normal = hand.PalmNormal;
            Vector direction = hand.Direction;
            Vector handSpeed = hand.PalmVelocity;
            float pinch = hand.PinchStrength;
            float pitch = direction.Pitch * 180.0f / (float)Math.PI;
            float roll = normal.Roll * 180.0f / (float)Math.PI;
            float yaw = direction.Yaw * 180.0f / (float)Math.PI;
            //Debug.Log(handPosition + "||" + newPosition + "||" + roll + "||" + handSpeed);
            if (-60 < roll && roll < 60 && handSpeed.z < - 500)
            {
                timeSinceLastCall = System.DateTime.Now;
                Debug.Log("Hand Up");
                Instanciate.singleton.gravityOff();
            } else if (-120 < roll && roll > 120 && handSpeed.z > 400)
            {
                timeSinceLastCall = System.DateTime.Now;
                Debug.Log("Hand down");
                Instanciate.singleton.gravityOn();
            }
            handPosition += (newPosition - handPosition) * 0.2f;
        }
    }



}

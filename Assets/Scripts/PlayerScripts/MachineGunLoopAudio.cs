using UnityEngine;

public class MachineGunLoopAudio : MonoBehaviour
{
    public AudioSource loopSource;
    public AudioClip startClip;
    public AudioClip endClip;

    bool playing;

    public void SetFiring(bool held)
    {
        if (held)
        {
            if (!playing)
            {
                if (startClip) AudioSource.PlayClipAtPoint(startClip, transform.position);
                if (loopSource) loopSource.Play();
                playing = true;
            }
        }
        else if (playing)
        {
            if (loopSource) loopSource.Stop();
            if (endClip) AudioSource.PlayClipAtPoint(endClip, transform.position);
            playing = false;
        }
    }
}

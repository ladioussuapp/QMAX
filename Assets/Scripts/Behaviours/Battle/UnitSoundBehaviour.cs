using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitSoundBehaviour : MonoBehaviour
{

    // Use this for initialization
    public GameObject ChargeSound;
    public GameObject AttackSound;
    public GameObject HitSound;
    public GameObject DieSound;
    public GameObject[] UpgradeSound;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetSoundSource(Dictionary<string, GameObject> sou)
    {
        if (sou == null)
        {
            return;
        }
        // AudioCharge
        if (sou.ContainsKey("AudioCharge"))
        {
            ChargeSound = sou["AudioCharge"];
        }
        // AudioAttack
        if (sou.ContainsKey("AudioAttack"))
        {
            AttackSound = sou["AudioAttack"];
        }
        // AudioDie
        if (sou.ContainsKey("AudioDie"))
        {
            DieSound = sou["AudioDie"];
        }
        // AudioHit
        if (sou.ContainsKey("AudioHit"))
        {
            HitSound = sou["AudioHit"];
        }
        // AudioUpgrade
        if (sou.ContainsKey("AudioUpgrade1"))
        {
            if (UpgradeSound == null)
            {
                UpgradeSound = new GameObject[3];
            }
            UpgradeSound[0] = sou["AudioUpgrade1"];
        }
        if (sou.ContainsKey("AudioUpgrade2"))
        {
            if (UpgradeSound == null)
            {
                UpgradeSound = new GameObject[3];
            }
            UpgradeSound[1] = sou["AudioUpgrade2"];
        }
        if (sou.ContainsKey("AudioUpgrade3"))
        {
            if (UpgradeSound == null)
            {
                UpgradeSound = new GameObject[3];
            }
            UpgradeSound[2] = sou["AudioUpgrade3"];
        }
    }

    public void PlayChargeSound()
    {
        if (ChargeSound == null)
        {
            return;
        }
        AudioSource charge = ChargeSound.GetComponent<AudioSource>();
        if (charge != null)
        {
            AudioClip ac = charge.clip;
            if (ac != null)
            {
                charge.Play();
            }
        }
    }

    public void PlayAttackSound()
    {
        if (AttackSound == null)
        {
            return;
        }
        AudioSource attack = AttackSound.GetComponent<AudioSource>();
        if (attack != null)
        {
            AudioClip ac = attack.clip;
            if (ac != null)
            {
                attack.Play();
            }
        }
    }

    public void PlayHitSound()
    {
        if (HitSound == null)
        {
            return;
        }
        AudioSource hit = HitSound.GetComponent<AudioSource>();
        if (hit != null)
        {
            AudioClip ac = hit.clip;
            if (ac != null)
            {
                hit.Play();
            }
        }
    }

    public void PlayDieSound()
    {
        if (DieSound == null)
        {
            return;
        }
        AudioSource die = DieSound.GetComponent<AudioSource>();
        if (die != null)
        {
            AudioClip ac = die.clip;
            if (ac != null)
            {
                die.Play();
            }
        }
    }

    public void PlayUpgradeSound()
    {
        if (UpgradeSound == null || UpgradeSound.Length == 0)
        {
            return;
        }
        int ranIndex = UnityEngine.Random.Range(0, UpgradeSound.Length);
        GameObject audio = UpgradeSound[ranIndex];
        AudioSource upgrade = audio.GetComponent<AudioSource>();
        if (upgrade != null)
        {
            AudioClip ac = upgrade.clip;
            if (ac != null)
            {
                upgrade.Play();
            }
        }
    }

    public AudioClip GetUpgradeAudioClip()
    {
        if (UpgradeSound == null || UpgradeSound.Length == 0)
        {
            return null;
        }

        int ranIndex = UnityEngine.Random.Range(0, UpgradeSound.Length);
        GameObject audio = UpgradeSound[ranIndex];
        AudioSource upgrade = audio.GetComponent<AudioSource>();
        if (upgrade != null)
        {
            return upgrade.clip;
        }

        return null;
    }
}

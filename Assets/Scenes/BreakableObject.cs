using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Header("Explosion Effect")]
    public GameObject destructionEffect;

    [Header("Explosion Sound")]
    public AudioClip explosionSound;
    public float soundVolume = 1f;

    private bool destroyed = false;

    public void DestroyBox()
    {
        if (destroyed) return;

        destroyed = true;

        // Play explosion sound before destroying the box
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(
                explosionSound,
                transform.position,
                soundVolume
            );
        }

        // Spawn destruction particles
        if (destructionEffect != null)
        {
            Instantiate(
                destructionEffect,
                transform.position,
                Quaternion.identity
            );
        }

        // Destroy the box
        Destroy(gameObject);
    }
}
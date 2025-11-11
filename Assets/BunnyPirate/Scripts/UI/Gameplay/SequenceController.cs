using UnityEngine;

public class SequenceController : MonoBehaviour
{
    public virtual void SequenceAwake()
    {
        GameManager.Register(this);
        ExitSequence();
    }

    public virtual void EnterSequence()
    {
        Debug.Log($"EnterSequence1");
    }

    public virtual void ExitSequence()
    {
    }
}

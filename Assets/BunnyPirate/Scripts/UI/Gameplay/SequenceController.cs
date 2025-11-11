using UnityEngine;

public class SequenceController : MonoBehaviour
{
    private int oddNum = 0;

    public int OddNum { get => oddNum; }

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

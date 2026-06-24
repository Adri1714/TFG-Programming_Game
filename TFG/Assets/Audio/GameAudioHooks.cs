using UnityEngine;

public class GameAudioHooks : MonoBehaviour
{
    private GameManager gm;

    private void Start()
    {
        gm = GameManager.Instance;
        if (gm == null) return;
        gm.OnOutputGenerated += OnOutput;
        gm.OnProgramFinished += OnFinished;
        gm.OnActionSucceeded += OnSuccess;
        gm.OnActionFailed    += OnFail;
    }

    private void OnDestroy()
    {
        if (gm == null) return;
        gm.OnOutputGenerated -= OnOutput;
        gm.OnProgramFinished -= OnFinished;
        gm.OnActionSucceeded -= OnSuccess;
        gm.OnActionFailed    -= OnFail;
    }

    private void OnOutput(string _) => AudioManager.Play(l => l.print);
    private void OnFinished()       => AudioManager.Play(l => l.levelComplete);
    private void OnFail()           => AudioManager.Play(l => l.error);

    private void OnSuccess(GameManager.TaskState s)
    {
        // El print (WRITE_IO) ja sona via OnOutput, així que aquí el saltem.
        AudioManager.Play(l => s switch
        {
            GameManager.TaskState.DIM_MEM   => l.declareVar,
            GameManager.TaskState.WRITE_MEM => l.assignVar,
            _ => null
        });
    }
}
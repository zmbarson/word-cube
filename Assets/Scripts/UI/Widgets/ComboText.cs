using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup), typeof(Text))]
public class ComboText : MonoBehaviour
{
    private CanvasGroup cgroup;
    private int         prevCombo;
    private Sequence    comboFade;
    private Text        label;

    private void Awake()
    {

        label        = GetComponent<Text>();
        cgroup       = GetComponent<CanvasGroup>();
        cgroup.alpha = 0f;
    }

    public void UpdateCount(int current, int max, int delta)
    {
        if (current == prevCombo || current == max) return;
        prevCombo  = current;
        label.text = $"{current}/{max}";
        StartCoroutine(DoAnimation());
    }

    IEnumerator DoAnimation()
    {
        comboFade ??= DOTween.Sequence()
           .Append(cgroup.DOFade(1f, 1f))
           .AppendInterval(1f)
           .Append(cgroup.DOFade(0f, 1f))
           .SetAutoKill(false);
        comboFade.Restart();
        yield return comboFade.WaitForCompletion();
    }
}

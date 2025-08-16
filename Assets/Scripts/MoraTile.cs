using UnityEngine;
using UnityEngine.UI;

public class MoraTile : MonoBehaviour
{
    public Image image;
    public string tileColor;
    public int tileNumber;
    [SerializeField] MoraJaiSolver moraJaiSolver;

    public void ClickTile()
    {
        if (!moraJaiSolver.isSolving && !moraJaiSolver.isSolved) moraJaiSolver.ClickTile(tileNumber);
    }
}

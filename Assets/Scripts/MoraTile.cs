using UnityEngine;
using UnityEngine.UI;

public class MoraTile : MonoBehaviour
{
    public Image image;
    public string tileColor;
    public int tileNumber;

    public void ClickTile()
    {
       MoraJaiSolver.instance.ClickTile(tileNumber); 
    }
}

using UnityEngine;

public class BuyPlotButtonBridge : MonoBehaviour
{
    public ChooseHouseUI chooseHouseUI;

    public void OnBuyPlotClicked()
    {
        if (PlotTrigger.ActivePlot != null)
        {
            chooseHouseUI.OpenPanel(PlotTrigger.ActivePlot.spawnPoint);
        }
    }
}
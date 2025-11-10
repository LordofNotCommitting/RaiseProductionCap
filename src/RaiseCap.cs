using HarmonyLib;
using MGSC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace CapProductionTime
{

    [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), nameof(MagnumSelectItemToProduceWindow.ReceiptPanelOnStartProduction))]
    public class RaiseCap
    {
        //steam mod ID 3594238447
        static int prod_Multiplier_Value = Plugin.ConfigGeneral.ModData.GetConfigValue<int>("Prod_Cap_Multiplier", 1);

        static bool Prefix(MagnumSelectItemToProduceWindow __instance, ItemReceiptPanel panel)
        {
            if (prod_Multiplier_Value == 1) {
                return true;
            }
            __instance._receiptToProduce = panel.Receipt;
            int maxCraft = ItemProductionSystem.GetAvailableToProduceCount<ItemStorage>(__instance._magnumCargo.ShipCargo, __instance._receiptToProduce.RequiredItems);
            int num = 1;
            CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(__instance._receiptToProduce.OutputItem, true) as CompositeItemRecord;
            foreach (BasePickupItemRecord basePickupItemRecord in compositeItemRecord.Records)
            {
                IStackableRecord stackableRecord = basePickupItemRecord as IStackableRecord;
                if (stackableRecord != null && compositeItemRecord.PrimaryRecord == basePickupItemRecord)
                {
                    num = (int)SingletonMonoBehaviour<ItemFactory>.Instance.GetMaxStackSize(stackableRecord);
                    break;
                }
            }
            bool flag = num > 1;
            bool flag2 = maxCraft == int.MaxValue;
            if (flag)
            {
                num = num * prod_Multiplier_Value;
                maxCraft = (flag2 ? num : Mathf.Min(maxCraft, num));
            }
            else
            {
                maxCraft = (flag2 ? 5 : Mathf.Min(maxCraft, 5));
            }
            UI.Chain<CommonContextMenu>().Invoke(delegate (CommonContextMenu v)
            {
                v.AddSliderCommand(maxCraft, 1, maxCraft);
            }).Invoke(delegate (CommonContextMenu v)
            {
                v.SetupCommand(Localization.Get("ui.context.StartProduction"), 0, true);
            }).Show(true).SetBackgroundOrder(-1).SetBackOnBackgroundClick(true);
            UI.Get<CommonContextMenu>().SnapToPosition(panel.transform.GetCenterPosition());
            return false;
        }



    }
}

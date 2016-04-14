using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Enhanced_Development.Temperature
{
    public class Building_PoweredVent : RimWorld.Building_Vent
    {

        CompPowerTrader power;


        //UI elements
        private static Texture2D UI_POWER_UP;
        private static Texture2D UI_POWER_UP_DISABLED;

        private static Texture2D UI_POWER_DOWN;
        private static Texture2D UI_POWER_DOWN_DISABLED;

        const int POWER_LEVEL_MIN = 0;
        const int POWER_LEVEL_MAX = 20;

        private int m_PowerLevel = 1;

        private const float EqualizationPercentPerTickRare = 0.25f;

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            UI_POWER_UP = ContentFinder<Texture2D>.Get("UI/PowerUp", true);
            UI_POWER_UP_DISABLED = ContentFinder<Texture2D>.Get("UI/PowerUp-BW", true);

            UI_POWER_DOWN = ContentFinder<Texture2D>.Get("UI/PowerDown", true);
            UI_POWER_DOWN_DISABLED = ContentFinder<Texture2D>.Get("UI/PowerDown-BW", true);


            this.power = base.GetComp<CompPowerTrader>();
        }

        public override void TickRare()
        {
            if (this.power.PowerOn)
            {
                IntVec3 loc1 = this.Position + Gen.RotatedBy(IntVec3.South, this.Rotation);
                IntVec3 loc2 = this.Position + Gen.RotatedBy(IntVec3.North, this.Rotation);
                Room room1 = GridsUtility.GetRoom(loc1);
                if (room1 == null)
                    return;
                Room room2 = GridsUtility.GetRoom(loc2);
                if (room2 == null || room1 == room2 || room1.UsesOutdoorTemperature && room2.UsesOutdoorTemperature)
                    return;
                float targetTemp = !room1.UsesOutdoorTemperature ? (!room2.UsesOutdoorTemperature ? (float)((double)room1.Temperature * (double)room1.CellCount + (double)room2.Temperature * (double)room2.CellCount) / (float)(room1.CellCount + room2.CellCount) : room2.Temperature) : room1.Temperature;
                if (!room1.UsesOutdoorTemperature)
                    this.Equalize(room1, targetTemp);
                if (room2.UsesOutdoorTemperature)
                    return;
                this.Equalize(room2, targetTemp);
            }
        }

        private void Equalize(Room r, float targetTemp)
        {
            float num = Mathf.Abs(r.Temperature - targetTemp) * 0.25f * this.m_PowerLevel;
            if ((double)targetTemp < (double)r.Temperature)
            {
                r.Temperature = Mathf.Max(targetTemp, r.Temperature - num);
            }
            else
            {
                if ((double)targetTemp <= (double)r.Temperature)
                    return;
                r.Temperature = Mathf.Min(targetTemp, r.Temperature + num);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            //Add the stock Gizmoes
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            
            if (true)
            {

                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.DecreasePower();
                if(this.m_PowerLevel > POWER_LEVEL_MIN)
                {
                    act.icon = UI_POWER_DOWN;
                }
                else
                {
                    act.icon = UI_POWER_DOWN_DISABLED;
                }
                act.defaultLabel = "Decrease Power";
                act.defaultDesc = "Decrease Power";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }

            if (true)
            {

                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.IncreasePower();
                if(this.m_PowerLevel < POWER_LEVEL_MAX)
                {
                    act.icon = UI_POWER_UP;
                }
                else
                {
                    act.icon = UI_POWER_UP_DISABLED;
                }
                act.defaultLabel = "Increase Power";
                act.defaultDesc = "Increase Power";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
        }

        private void IncreasePower()
        {
            if (this.m_PowerLevel < Building_PoweredVent.POWER_LEVEL_MAX)
            {
                this.m_PowerLevel += 1;
            }
            this.updatePowerUsage();
        }

        private void DecreasePower()
        {
            if (this.m_PowerLevel > Building_PoweredVent.POWER_LEVEL_MIN)
            {
                this.m_PowerLevel -= 1;
            }
            this.updatePowerUsage();
        }
        
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Power Level: " + this.m_PowerLevel);
            stringBuilder.AppendLine(base.GetInspectString());

            return stringBuilder.ToString();
        }

        public void updatePowerUsage()
        {
            this.power.PowerOutput = this.m_PowerLevel * -1 * 100;
        }

        //Saving game
        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.LookValue(ref this.m_PowerLevel, "m_PowerLevel");
        }

    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Values : MonoBehaviour {
    public Pipe pipe;
    // Update is called once per frame
    ValueSetter[] vss;
    GameObject[] all;

    string[] flagreg = { "%eax", "%ecx", "%edx", "%ebx", "%esp", "%ebp", "%esi", "%edi" };
    string[] flagstat = { "SAOK", "SADR", "SINS", "SHLT", "SBUB", "SSTA" };
    public Dictionary<string, string> value;

    void Start() {
        all = GameObject.FindGameObjectsWithTag("ValueSetter");
        vss = new ValueSetter[105];
        int j = 0;
        foreach (GameObject i in all)
            vss[j++] = i.GetComponent<ValueSetter>();

        value = new Dictionary<string, string>();
    }
    public void allUpdate(int clock_cnt)
    {
        updateValue(clock_cnt);
    }
    public void triggerUpdate(int part)
    {
        int len = all.Length;
        Debug.Log("triggerupte" + len.ToString() + " " + part.ToString());
        int partlen = len / 10 + 1;
        for(int i = partlen * part; i < len && i < partlen * (part+1); i++)
            all[i].GetComponent<ValueSetter>().myupdate();
    }

    void updateValue(int clock_cnt) {


        value["log"] = pipe.print_out(clock_cnt);

        value["f_predPC"] = string.Format("0x{0:x8}", pipe.rf_predPC[clock_cnt]);
        value["F_predPC"] = string.Format("0x{0:x8}", pipe.rF_predPC[clock_cnt]);

        value["D_stat"] = string.Format("{0}", flagstat[pipe.rD_stat[clock_cnt]]);
        value["D_icode"] = string.Format("0x{0:x}", pipe.rD_icode[clock_cnt]);
        value["D_ifun"] = string.Format("0x{0:x}", pipe.rD_ifun[clock_cnt]);
        value["D_valC"] = string.Format("0x{0:x8}", pipe.rD_valC[clock_cnt]);
        value["D_valP"] = string.Format("0x{0:x8}", pipe.rD_valP[clock_cnt]);
        value["D_rA"] = string.Format("0x{0:x}", pipe.rD_rA[clock_cnt]);
        value["D_rB"] = string.Format("0x{0:x}", pipe.rD_rB[clock_cnt]);
        value["d_dstE"] = string.Format("0x{0:x}", pipe.rd_dstE[clock_cnt]);
        value["d_dstM"] = string.Format("0x{0:x}", pipe.rd_dstM[clock_cnt]);
        value["d_srcA"] = string.Format("0x{0:x}", pipe.rd_srcA[clock_cnt]);
        value["d_srcB"] = string.Format("0x{0:x}", pipe.rd_srcB[clock_cnt]);

        StringWriter sw = new StringWriter();
        for (int i = 0; i < 8; i++)
            sw.WriteLine(flagreg[i] + " = {0:x8}", pipe.rreg_file[clock_cnt, i]);
        value["reg"] = sw.ToString();

        value["e_Cnd"] = string.Format("{0}", Convert.ToString(pipe.re_Cnd[clock_cnt]).ToLower());
        value["e_dstE"] = string.Format("0x{0:x}", pipe.re_dstE[clock_cnt]);
        value["E_stat"] = string.Format("{0}", flagstat[pipe.rE_stat[clock_cnt]]);
        value["E_icode"] = string.Format("0x{0:x}", pipe.rE_icode[clock_cnt]);
        value["E_ifun"] = string.Format("0x{0:x}", pipe.rE_ifun[clock_cnt]);
        value["E_valC"] = string.Format("0x{0:x8}", pipe.rE_valC[clock_cnt]);
        value["E_valA"] = string.Format("0x{0:x8}", pipe.rE_valA[clock_cnt]);
        value["E_valB"] = string.Format("0x{0:x8}", pipe.rE_valB[clock_cnt]);
        value["E_dstE"] = string.Format("0x{0:x}", pipe.rE_dstE[clock_cnt]);
        value["E_dstM"] = string.Format("0x{0:x}", pipe.rE_dstM[clock_cnt]);
        value["E_srcA"] = string.Format("0x{0:x}", pipe.rE_srcA[clock_cnt]);
        value["E_srcB"] = string.Format("0x{0:x}", pipe.rE_srcB[clock_cnt]);

        sw = new StringWriter();
        sw.WriteLine("SF = {0}", Convert.ToString(pipe.rSF[clock_cnt]).ToLower());
        sw.WriteLine("ZF = {0}", Convert.ToString(pipe.rZF[clock_cnt]).ToLower());
        sw.WriteLine("OF = {0}", Convert.ToString(pipe.rOF[clock_cnt]).ToLower());
        value["CC"] = sw.ToString();

        value["M_stat"] = string.Format("{0}", flagstat[pipe.rM_stat[clock_cnt]]);
        value["M_icode"] = string.Format("0x{0:x}", pipe.rM_icode[clock_cnt]);
        value["M_valE"] = string.Format("0x{0:x8}", pipe.rM_valE[clock_cnt]);
        value["M_valA"] = string.Format("0x{0:x8}", pipe.rM_valA[clock_cnt]);
        value["M_dstE"] = string.Format("0x{0:x}", pipe.rM_dstE[clock_cnt]);
        value["M_dstM"] = string.Format("0x{0:x}", pipe.rM_dstM[clock_cnt]);
        value["M_Cnd"] = string.Format("{0}", Convert.ToString(pipe.rM_Cnd[clock_cnt]).ToLower());

        value["W_stat"] = string.Format("{0}", flagstat[pipe.rW_stat[clock_cnt]]);
        value["W_icode"] = string.Format("0x{0:x}", pipe.rW_icode[clock_cnt]);
        value["W_valE"] = string.Format("0x{0:x8}", pipe.rW_valE[clock_cnt]);
        value["W_valM"] = string.Format("0x{0:x8}", pipe.rW_valM[clock_cnt]);
        value["W_dstE"] = string.Format("0x{0:x}", pipe.rW_dstE[clock_cnt]);
        value["W_dstM"] = string.Format("0x{0:x}", pipe.rW_dstM[clock_cnt]);
        value["Stat"] = string.Format("{0}", Convert.ToString(pipe.rStat[clock_cnt]).ToLower());

        value["Cycle:"] = string.Format("{0}",clock_cnt);
    }

}

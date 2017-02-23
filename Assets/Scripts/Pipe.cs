using System;
using System.IO;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Config config;
    public void Start()
    {
        initial_Y86Pipeline();
    }
    public void run(string filePath)
    {
        runpipe(filePath);
        Debug.Log("Runfinish");
        Config.going = true;
        config.mystart();
    }
    #region read to mem
    int tot_instr;
    byte[] mem = new byte[5000];
    public void read(string filepath)
    {
        FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        BinaryReader br = new BinaryReader(fs);
        int length = (int)fs.Length;
        int i = 0;
        while (length > 0)
        {
            byte tempByte = br.ReadByte();
            mem[i] = tempByte;
            string tempStr = mem[i].ToString("x2");
            i++;
            length--;
        }
        tot_instr = i;
        fs.Close();
        br.Close();
    }
    #endregion
    #region variable
    string[] reg = { "%eax", "%ecx", "%edx", "%ebx", "%esp", "%ebp", "%esi", "%edi" };
    string[] stat = { "SAOK", "SADR", "SINS", "SHLT", "SBUB", "SSTA" };
    const int MAX_MEM_CAP = (1 << 16);//the allocated memory capacity
    //public byte[] mem;//memory for instructions and data
    int[] reg_file;//register file
    public int cycle_cnt;
    //the pipeline variable. 
    //Upper first letter is the pipeline registers' variable, e.g. "D_stat"
    //Lower first letter is the pipeline stages' variable, e.g. "d_valA"
    int F_predPC;
    byte[] instr_mem = new byte[6];
    byte Split;
    byte[] Align = new byte[5];
    int f_stat, f_icode, f_ifun, f_valC, f_valP, f_pc, f_predPC;
    byte f_rA, f_rB;
    bool imem_error, instr_valid, need_regids, need_valC;
    int D_stat, D_icode, D_ifun, D_rA, D_rB, D_valC, D_valP;
    int d_stat, d_icode, d_ifun, d_valC, d_valA, d_valB, d_dstE, d_dstM, d_srcA, d_srcB;
    int E_stat, E_icode, E_ifun, E_valC, E_valA, E_valB, E_dstE, E_dstM, E_srcA, E_srcB;
    int e_stat, e_icode, e_valE, e_valA, e_dstE, e_dstM;
    bool e_Cnd;
    int M_stat, M_icode, M_valE, M_valA, M_dstE, M_dstM;
    bool M_Cnd;
    int m_stat, m_icode, m_valE, m_valM, m_dstE, m_dstM;
    bool dmem_error;
    int W_stat, W_icode, W_valE, W_valM, W_dstE, W_dstM;
    int Stat;
    int w_dstE, w_valE, w_dstM, w_valM;
    byte imem_icode, imem_ifun;
    //Pipeline register control signals
    bool F_stall, F_bubble, D_stall, D_bubble, E_stall, E_bubble, M_stall, M_bubble, W_stall, W_bubble;
    string Forwarding_A, Forwarding_B;
    bool ZF, SF, OF;//condition code
    int aluA, aluB;
    int alufun;
    bool set_cc;
    int mem_addr;
    bool mem_read, mem_write;

    //record
    public int[] rF_predPC, rf_stat, rf_icode, rf_ifun, rf_valC, rf_valP, rf_pc, rf_predPC;
    public byte[] rf_rA, rf_rB;
    public bool[] rimem_error, rinstr_valid, rneed_regids, rneed_valC;
    public int[] rD_stat, rD_icode, rD_ifun, rD_rA, rD_rB, rD_valC, rD_valP;
    public int[] rd_stat, rd_icode, rd_ifun, rd_valC, rd_valA, rd_valB, rd_dstE, rd_dstM, rd_srcA, rd_srcB;
    public int[] rE_stat, rE_icode, rE_ifun, rE_valC, rE_valA, rE_valB, rE_dstE, rE_dstM, rE_srcA, rE_srcB;
    public int[] re_stat, re_icode, re_valE, re_valA, re_dstE, re_dstM;
    public bool[] re_Cnd;
    public int[] rM_stat, rM_icode, rM_valE, rM_valA, rM_dstE, rM_dstM;
    public bool[] rM_Cnd;
    public int[] rm_stat, rm_icode, rm_valE, rm_valM, rm_dstE, rm_dstM;
    public bool[] rdmem_error;
    public int[] rW_stat, rW_icode, rW_valE, rW_valM, rW_dstE, rW_dstM;
    public int[] rStat;
    public int[] rw_dstE, rw_valE, rw_dstM, rw_valM;
    public byte[] rimem_icode, rimem_ifun;


    //Pipeline register control signals
    public bool[] rF_stall, rF_bubble, rD_stall, rD_bubble, rE_stall, rE_bubble, rM_stall, rM_bubble, rW_stall, rW_bubble;
    public string[] rForwarding_A, rForwarding_B;
    public bool[] rZF, rSF, rOF;//condition code
    public int[] raluA, raluB;
    public int[] ralufun;
    public bool[] rset_cc;
    public int[] rmem_addr;
    public bool[] rmem_read, rmem_write;
    public int[,] rreg_file;

    //record initial
    void record_initial()
    {
        rF_predPC = new int[MAX_MEM_CAP];
        rf_stat = new int[MAX_MEM_CAP];
        rf_icode = new int[MAX_MEM_CAP];
        rf_ifun = new int[MAX_MEM_CAP];
        rf_valC = new int[MAX_MEM_CAP];
        rf_valP = new int[MAX_MEM_CAP];
        rf_pc = new int[MAX_MEM_CAP];
        rf_predPC = new int[MAX_MEM_CAP];
        rD_stat = new int[MAX_MEM_CAP];
        rD_icode = new int[MAX_MEM_CAP];
        rD_ifun = new int[MAX_MEM_CAP];
        rD_rA = new int[MAX_MEM_CAP];
        rD_rB = new int[MAX_MEM_CAP];
        rD_valC = new int[MAX_MEM_CAP];
        rD_valP = new int[MAX_MEM_CAP];
        rd_stat = new int[MAX_MEM_CAP];
        rd_icode = new int[MAX_MEM_CAP];
        rd_ifun = new int[MAX_MEM_CAP];
        rd_valC = new int[MAX_MEM_CAP];
        rd_valA = new int[MAX_MEM_CAP];
        rd_valB = new int[MAX_MEM_CAP];
        rd_dstE = new int[MAX_MEM_CAP];
        rd_dstM = new int[MAX_MEM_CAP];
        rd_srcA = new int[MAX_MEM_CAP];
        rd_srcB = new int[MAX_MEM_CAP];
        rE_stat = new int[MAX_MEM_CAP];
        rE_icode = new int[MAX_MEM_CAP];
        rE_ifun = new int[MAX_MEM_CAP];
        rE_valC = new int[MAX_MEM_CAP];
        rE_valA = new int[MAX_MEM_CAP];
        rE_valB = new int[MAX_MEM_CAP];
        rE_dstE = new int[MAX_MEM_CAP];
        rE_dstM = new int[MAX_MEM_CAP];
        rE_srcA = new int[MAX_MEM_CAP];
        rE_srcB = new int[MAX_MEM_CAP];
        re_stat = new int[MAX_MEM_CAP];
        re_icode = new int[MAX_MEM_CAP];
        re_valE = new int[MAX_MEM_CAP];
        re_valA = new int[MAX_MEM_CAP];
        re_dstE = new int[MAX_MEM_CAP];
        re_dstM = new int[MAX_MEM_CAP];
        rM_stat = new int[MAX_MEM_CAP];
        rM_icode = new int[MAX_MEM_CAP];
        rM_valE = new int[MAX_MEM_CAP];
        rM_valA = new int[MAX_MEM_CAP];
        rM_dstE = new int[MAX_MEM_CAP];
        rM_dstM = new int[MAX_MEM_CAP];
        rm_stat = new int[MAX_MEM_CAP];
        rm_icode = new int[MAX_MEM_CAP];
        rm_valE = new int[MAX_MEM_CAP];
        rm_valM = new int[MAX_MEM_CAP];
        rm_dstE = new int[MAX_MEM_CAP];
        rm_dstM = new int[MAX_MEM_CAP];
        rW_stat = new int[MAX_MEM_CAP];
        rW_icode = new int[MAX_MEM_CAP];
        rW_valE = new int[MAX_MEM_CAP];
        rW_valM = new int[MAX_MEM_CAP];
        rW_dstE = new int[MAX_MEM_CAP];
        rW_dstM = new int[MAX_MEM_CAP];
        rStat = new int[MAX_MEM_CAP];
        rw_dstE = new int[MAX_MEM_CAP];
        rw_valE = new int[MAX_MEM_CAP];
        rw_dstM = new int[MAX_MEM_CAP];
        rw_valM = new int[MAX_MEM_CAP];
        raluA = new int[MAX_MEM_CAP];
        raluB = new int[MAX_MEM_CAP];
        ralufun = new int[MAX_MEM_CAP];
        rmem_addr = new int[MAX_MEM_CAP];
        rf_rA = new byte[MAX_MEM_CAP];
        rf_rB = new byte[MAX_MEM_CAP];
        rimem_error = new bool[MAX_MEM_CAP];
        rinstr_valid = new bool[MAX_MEM_CAP];
        rneed_regids = new bool[MAX_MEM_CAP];
        rneed_valC = new bool[MAX_MEM_CAP];
        re_Cnd = new bool[MAX_MEM_CAP];
        rM_Cnd = new bool[MAX_MEM_CAP];
        rdmem_error = new bool[MAX_MEM_CAP];
        rimem_icode = new byte[MAX_MEM_CAP];
        rimem_ifun = new byte[MAX_MEM_CAP];
        rF_stall = new bool[MAX_MEM_CAP];
        rF_bubble = new bool[MAX_MEM_CAP];
        rD_stall = new bool[MAX_MEM_CAP];
        rD_bubble = new bool[MAX_MEM_CAP];
        rE_stall = new bool[MAX_MEM_CAP];
        rE_bubble = new bool[MAX_MEM_CAP];
        rM_stall = new bool[MAX_MEM_CAP];
        rM_bubble = new bool[MAX_MEM_CAP];
        rW_stall = new bool[MAX_MEM_CAP];
        rW_bubble = new bool[MAX_MEM_CAP];
        rForwarding_A = new string [MAX_MEM_CAP];
        rForwarding_B = new string[MAX_MEM_CAP];
        rZF = new bool[MAX_MEM_CAP];
        rSF = new bool[MAX_MEM_CAP];
        rOF = new bool[MAX_MEM_CAP];
        rset_cc = new bool[MAX_MEM_CAP];
        rmem_read = new bool[MAX_MEM_CAP];
        rmem_write = new bool[MAX_MEM_CAP];
        rreg_file = new int[MAX_MEM_CAP,8];
    }

    //save record 
    void record_save(int i)
    {
        rF_predPC[i] = F_predPC;
        rf_stat[i] = f_stat;
        rf_icode[i] = f_icode;
        rf_ifun[i] = f_ifun;
        rf_valC[i] = f_valC;
        rf_valP[i] = f_valP;
        rf_pc[i] = f_pc;
        rf_predPC[i] = f_predPC;
        rD_stat[i] = D_stat;
        rD_icode[i] = D_icode;
        rD_ifun[i] = D_ifun;
        rD_rA[i] = D_rA;
        rD_rB[i] = D_rB;
        rD_valC[i] = D_valC;
        rD_valP[i] = D_valP;
        rd_stat[i] = d_stat;
        rd_icode[i] = d_icode;
        rd_ifun[i] = d_ifun;
        rd_valC[i] = d_valC;
        rd_valA[i] = d_valA;
        rd_valB[i] = d_valB;
        rd_dstE[i] = d_dstE;
        rd_dstM[i] = d_dstM;
        rd_srcA[i] = d_srcA;
        rd_srcB[i] = d_srcB;
        rE_stat[i] = E_stat;
        rE_icode[i] = E_icode;
        rE_ifun[i] = E_ifun;
        rE_valC[i] = E_valC;
        rE_valA[i] = E_valA;
        rE_valB[i] = E_valB;
        rE_dstE[i] = E_dstE;
        rE_dstM[i] = E_dstM;
        rE_srcA[i] = E_srcA;
        rE_srcB[i] = E_srcB;
        re_stat[i] = e_stat;
        re_icode[i] = e_icode;
        re_valE[i] = e_valE;
        re_valA[i] = e_valA;
        re_dstE[i] = e_dstE;
        re_dstM[i] = e_dstM;
        rM_stat[i] = M_stat;
        rM_icode[i] = M_icode;
        rM_valE[i] = M_valE;
        rM_valA[i] = M_valA;
        rM_dstE[i] = M_dstE;
        rM_dstM[i] = M_dstM;
        rm_stat[i] = m_stat;
        rm_icode[i] = m_icode;
        rm_valE[i] = m_valE;
        rm_valM[i] = m_valM;
        rm_dstE[i] = m_dstE;
        rm_dstM[i] = m_dstM;
        rW_stat[i] = W_stat;
        rW_icode[i] = W_icode;
        rW_valE[i] = W_valE;
        rW_valM[i] = W_valM;
        rW_dstE[i] = W_dstE;
        rW_dstM[i] = W_dstM;
        rStat[i] = Stat;
        rw_dstE[i] = w_dstE;
        rw_valE[i] = w_valE;
        rw_dstM[i] = w_dstM;
        rw_valM[i] = w_valM;
        raluA[i] = aluA;
        raluB[i] = aluB;
        ralufun[i] = alufun;
        rmem_addr[i] = mem_addr;
        rf_rA[i] = f_rA;
        rf_rB[i] = f_rB;
        rimem_error[i] = imem_error;
        rinstr_valid[i] = instr_valid;
        rneed_regids[i] = need_regids;
        rneed_valC[i] = need_valC;
        re_Cnd[i] = e_Cnd;
        rM_Cnd[i] = M_Cnd;
        rdmem_error[i] = dmem_error;
        rimem_icode[i] = imem_icode;
        rimem_ifun[i] = imem_ifun;
        rF_stall[i] = F_stall;
        rF_bubble[i] = F_bubble;
        rD_stall[i] = D_stall;
        rD_bubble[i] = D_bubble;
        rE_stall[i] = E_stall;
        rE_bubble[i] = E_bubble;
        rM_stall[i] = M_stall;
        rM_bubble[i] = M_bubble;
        rW_stall[i] = W_stall;
        rW_bubble[i] = W_bubble;
        rForwarding_A[i] = Forwarding_A;
        rForwarding_B[i] = Forwarding_B;
        rZF[i] = ZF;
        rSF[i] = SF;
        rOF[i] = OF;
        rset_cc[i] = set_cc;
        rmem_read[i] = mem_read;
        rmem_write[i] = mem_write;
        for (int j = 0; j < 8; j++)
            rreg_file[i,j] = reg_file[j];
    }

    //HCL
    const int INOP = 0;//nop
    const int IHALT = 1;//halt
    const int IRRMOVL = 2;//rrmovl
    const int IIRMOVL = 3;//rimovl
    const int IRMMOVL = 4;//rmmovl
    const int IMRMOVL = 5;//mrmovl
    const int IOPL = 6;//integer arthmetic
    const int IJXX = 7;//jump instruction
    const int ICALL = 8;//call
    const int IRET = 9;//ret
    const int IPUSHL = 0xA;//pushl
    const int IPOPL = 0xB;//popl
    const int FNONE = 0;//default function
    const int ALUADD = 0;//add
    const int SAOK = 1;//normal
    const int SADR = 2;//abnormal address 
    const int SINS = 3;//illegal instruction
    const int SHLT = 4;//halt
    const int SBUB = 5;//bubble
    const int SSTA = 6;//stalling
    const int REAX = 0;//%eax
    const int RECX = 1;//%ecx
    const int REDX = 2;//%edx
    const int REBX = 3;//%ebx
    const int RESP = 4;//%esp
    const int REBP = 5;//%ebp
    const int RESI = 6;//%esi
    const int REDI = 7;//%edi
    const int RNONE = 0xF;//no register
    int[] register = new int[20];  //real value of registers;
    #endregion
    #region initial
    public Pipe()
    {
        initial_Y86Pipeline();
    }
    public void initial_Y86Pipeline()      //initialise pipeline
    {
        cycle_cnt = 0;
        F_predPC = f_icode = f_ifun = f_valC = f_valP = f_pc = f_predPC = 0;
        D_icode = D_ifun = D_valC = D_valP = 0;
        d_icode = d_ifun = d_valC = d_valA = d_valB = 0;
        E_icode = E_ifun = E_valC = E_valA = E_valB = 0;
        e_icode = e_valE = 0;
        M_icode = M_valE = M_valA = 0;
        m_icode = m_valE = m_valM = 0;
        W_icode = W_valE = W_valM = 0;
        aluA = aluB = alufun = mem_addr = 0;
        D_rA = D_rB = d_dstE = d_dstM = d_srcA = d_srcB = RNONE;
        E_dstE = E_dstM = E_srcA = E_srcB = e_dstE = e_dstM = RNONE;
        M_dstE = M_dstM = m_dstE = m_dstM = RNONE;
        W_dstE = W_dstM = RNONE;
        imem_icode = INOP;
        imem_ifun = FNONE;
        f_stat = D_stat = d_stat = E_stat = e_stat = M_stat = m_stat = W_stat = Stat = SAOK;
        f_rA = f_rB = REAX;
        imem_error = instr_valid = dmem_error = ZF = SF = OF = e_Cnd = M_Cnd = set_cc = mem_read = mem_write = false;
        F_stall = F_bubble = D_stall = D_bubble = E_stall = E_bubble = M_bubble = M_stall = W_bubble = W_stall = false;
        Forwarding_A = Forwarding_B = "N";
        mem = new byte[MAX_MEM_CAP];
        for (int i = 0; i < MAX_MEM_CAP; i++)
            mem[i] = 0;
        reg_file = new int[9];
        for (int i = 0; i < 9; i++)
            reg_file[i] = 0;
    }
    #endregion
    #region Fetch Stage
    void Fetch()
    {
        //Select PC module: What address should instruction be fetched at
        if (M_icode == IJXX && !M_Cnd)
            f_pc = M_valA;
        else if (W_icode == IRET)
            f_pc = W_valM;
        else
            f_pc = F_predPC;
        //Instruction memory & Split & Align
        for (int i = 0; i < 6; i++)
            instr_mem[i] = mem[f_pc + i];
        Split = instr_mem[0];
        imem_icode = (byte)((Split >> 4) & (0xf));
        imem_ifun = (byte)(Split & (0xf));
        for (int i = 0; i < 5; i++)
            Align[i] = instr_mem[1 + i];
        if (f_pc > MAX_MEM_CAP)
            imem_error = true;
        else
            imem_error = false;
        //Determine icode of fetched instruction
        if (imem_error)
            f_icode = INOP;
        else
            f_icode = imem_icode;
        //Determine ifun
        if (imem_error)
            f_ifun = FNONE;
        else
            f_ifun = imem_ifun;
        //Is instruction valid?
        if (f_icode == INOP || f_icode == IHALT || f_icode == IRRMOVL || f_icode == IIRMOVL || f_icode == IRMMOVL || f_icode == IMRMOVL || f_icode == IOPL || f_icode == IJXX || f_icode == ICALL || f_icode == IRET || f_icode == IPUSHL || f_icode == IPOPL)
            instr_valid = true;
        else
            instr_valid = false;
        //Does fetched instruction require a regid byte?
        if (f_icode == IRRMOVL || f_icode == IOPL || f_icode == IPUSHL || f_icode == IPOPL || f_icode == IIRMOVL || f_icode == IRMMOVL || f_icode == IMRMOVL)
            need_regids = true;
        else
            need_regids = false;
        //Does fetched instruction require a constant word?
        if (f_icode == IIRMOVL || f_icode == IRMMOVL || f_icode == IMRMOVL || f_icode == IJXX || f_icode == ICALL)
            need_valC = true;
        else
            need_valC = false;
        //Determine status code for fetched instruction
        if (imem_error)
            f_stat = SADR;
        else if (!instr_valid)
            f_stat = SINS;
        else if (f_icode == IHALT)
            f_stat = SHLT;
        else
            f_stat = SAOK;
        //fetch module
        if (need_regids)
        {
            f_rA = (byte)((Align[0] >> 4) & (0xf));
            f_rB = (byte)(Align[0] & (0xf));
            f_valC = 0;
            for (int i = 0; i < 4; i++)
            {
                f_valC <<= 8;
                f_valC += Align[4 - i];
            }
        }
        else
        {
            f_rA = RNONE;
            f_rB = RNONE;
            f_valC = 0;
            for (int i = 0; i < 4; i++)
            {
                f_valC <<= 8;
                f_valC += Align[3 - i];
            }
        }
        //PC increment module
        if (need_valC && need_regids)
            f_valP = f_pc + 6;
        else if (need_valC && !need_regids)
            f_valP = f_pc + 5;
        else if (!need_valC && need_regids)
            f_valP = f_pc + 2;
        else
            f_valP = f_pc + 1;
        //Predict next value of PC
        if (f_icode == IJXX || f_icode == ICALL)
            f_predPC = f_valC;
        else
            f_predPC = f_valP;
    }
    #endregion
    #region Decode Stage
    void Decode()
    {
        d_stat = D_stat;
        d_icode = D_icode;
        d_ifun = D_ifun;
        d_valC = D_valC;
        //What register should be used as the A source?
        if (D_icode == IRRMOVL || D_icode == IRMMOVL || D_icode == IOPL || D_icode == IPUSHL)
            d_srcA = D_rA;
        else if (D_icode == IPOPL || D_icode == IRET)
            d_srcA = RESP;
        else
            d_srcA = RNONE;//Don’t need register
        //What register should be used as the B source?
        if (D_icode == IOPL || D_icode == IRMMOVL || D_icode == IMRMOVL)
            d_srcB = D_rB;
        else if (D_icode == IPUSHL || D_icode == IPOPL || D_icode == ICALL || D_icode == IRET)
            d_srcB = RESP;
        else
            d_srcB = RNONE;//Don’t need register
        //What register should be used as the E destination?
        if (D_icode == IRRMOVL || D_icode == IIRMOVL || D_icode == IOPL)
            d_dstE = D_rB;
        else if (D_icode == IPUSHL || D_icode == IPOPL || D_icode == ICALL || D_icode == IRET)
            d_dstE = RESP;
        else
            d_dstE = RNONE;// Don’t write any register
        //What register should be used as the M destination?
        if (D_icode == IMRMOVL || D_icode == IPOPL)
            d_dstM = D_rA;
        else
            d_dstM = RNONE;// Don’t write any register
        //What should be the A value?
        //Forward into decode stage for valA
        //Sel+Fwd A module
        if (D_icode == ICALL || D_icode == IJXX)
        {
            d_valA = D_valP;
            Forwarding_A = "NULL";
        }
        else if (d_srcA == e_dstE)
        {
            d_valA = e_valE;
            Forwarding_A = "e_valE:0x" + e_valE.ToString("x8");
        }
        else if (d_srcA == M_dstM)
        {
            d_valA = m_valM;
            Forwarding_A = "m_valM:0x" + m_valM.ToString("x8");
        }
        else if (d_srcA == M_dstE)
        {
            d_valA = M_valE;
            Forwarding_A = "M_valE:0x" + M_valE.ToString("x8");
        }
        else if (d_srcA == W_dstM)
        {
            d_valA = W_valM;
            Forwarding_A = "W_valM:0x" + W_valM.ToString("x8");
        }
        else if (d_srcA == W_dstE)
        {
            d_valA = W_valE;
            Forwarding_A = "W_valE:0x" + W_valE.ToString("x8");
        }
        else
        {
            d_valA = reg_file[d_srcA];
            Forwarding_A = "NULL";
        }
        //Fwd B module
        if (d_srcB == e_dstE)
        {
            d_valB = e_valE;
            Forwarding_B = "e_valE:0x" + e_valE.ToString("x8");
        }
        else if (d_srcB == M_dstM)
        {
            d_valB = m_valM;
            Forwarding_B = "m_valM:0x" + m_valM.ToString("x8");
        }
        else if (d_srcB == M_dstE)
        {
            d_valB = M_valE;
            Forwarding_B = "M_valE:0x" + M_valE.ToString("x8");
        }
        else if (d_srcB == W_dstM)
        {
            d_valB = W_valM;
            Forwarding_B = "W_valM:0x" + W_valM.ToString("x8");
        }
        else if (d_srcB == W_dstE)
        {
            d_valB = W_valE;
            Forwarding_B = "W_valE:0x" + W_valE.ToString("x8");
        }
        else
        {
            d_valB = reg_file[d_srcB];
            Forwarding_B = "NULL";
        }
    }
    #endregion
    #region Execute Stage
    void Execute()
    {
        e_stat = E_stat;
        e_icode = E_icode;
        //Select input A to ALU
        if (E_icode == IRRMOVL || E_icode == IOPL)
            aluA = E_valA;
        else if (E_icode == IRRMOVL || E_icode == IRMMOVL || E_icode == IMRMOVL)
            aluA = E_valC;
        else if (E_icode == ICALL || E_icode == IPUSHL)
            aluA = -4;
        else if (E_icode == IRET || E_icode == IPOPL)
            aluA = 4;
        else
            aluA = 0;//Other instructions don’t need ALU
        //Select input B to ALU
        if (E_icode == IRMMOVL || E_icode == IMRMOVL || E_icode == IOPL || E_icode == ICALL || E_icode == IPUSHL || E_icode == IRET || E_icode == IPOPL)
            aluB = E_valB;
        else if (E_icode == IRRMOVL || E_icode == IIRMOVL)
            aluB = 0;
        else
            aluB = 0;//Other instructions don’t need ALU
        //Set the ALU function
        if (E_icode == IOPL)
            alufun = E_ifun;
        else
            alufun = ALUADD;
        //ALU module
        switch (alufun)
        {
            case 0: e_valE = aluB + aluA; break;
            case 1: e_valE = aluB - aluA; break;
            case 2: e_valE = aluB & aluA; break;
            case 3: e_valE = aluB ^ aluA; break;
            default: e_valE = 0; break;
        }
        //Should the condition codes be updated?
        //State changes only during normal operation
        if (E_icode == IOPL && m_stat != SADR && m_stat != SINS && m_stat != SHLT && W_stat != SADR && W_stat != SINS && W_stat != SHLT)
            set_cc = true;
        else
            set_cc = false;
        //Generate valA in execute stage
        e_valA = E_valA;
        //Set dstE to RNONE in event of not-taken conditional move
        if (E_icode == IRRMOVL && !e_Cnd)
            e_dstE = RNONE;
        else
            e_dstE = E_dstE;
        //cc
        if (set_cc)
        {
            if (e_valE < 0)
                SF = true;
            else
                SF = false;
            if (e_valE == 0)
                ZF = true;
            else
                ZF = false;
            if ((aluA < 0 == aluB < 0) && (aluA < 0 != e_valE < 0))
                OF = true;
            else
                OF = false;
        }
        //e_Cnd module            
        if (E_icode == IJXX || E_icode == IRRMOVL)
        {
            switch (E_ifun)
            {
                case 0: e_Cnd = true; break;//jmp or rrmovl
                case 1: if (ZF || SF) e_Cnd = true; else e_Cnd = false; break;//jle or cmovle
                case 2: if (SF) e_Cnd = true; else e_Cnd = false; break;//jl or cmovl
                case 3: if (ZF) e_Cnd = true; else e_Cnd = false; break;//je or comve
                case 4: if (!ZF) e_Cnd = true; else e_Cnd = false; break;//jne or cmovne
                case 5: if (!SF) e_Cnd = true; else e_Cnd = false; break;//jge or comvge
                case 6: if (!ZF && !SF) e_Cnd = true; else e_Cnd = false; break;//jg or comvg
            }
        }
        else
            e_Cnd = false;
        e_dstM = E_dstM;
    }
    #endregion
    #region Memory Stage
    void Memory()
    {
        m_icode = M_icode;
        m_valE = M_valE;
        m_dstE = M_dstE;
        m_dstM = M_dstM;
        //Select memory address
        if (M_icode == IRMMOVL || M_icode == IPUSHL || M_icode == ICALL || M_icode == IMRMOVL)
            mem_addr = M_valE;
        else if (M_icode == IPOPL || M_icode == IRET)
            mem_addr = M_valA;//Other instructions don’t need address
        //Set read control signal
        if (M_icode == IMRMOVL || M_icode == IPOPL || M_icode == IRET)
            mem_read = true;
        else
            mem_read = false;
        //Set write control signal
        if (M_icode == IRMMOVL || M_icode == IPUSHL || M_icode == ICALL)
            mem_write = true;
        else
            mem_write = false;
        //read / write memory
        if (mem_addr > MAX_MEM_CAP)
            dmem_error = true;
        else
        {
            dmem_error = false;
            if (mem_read)
            {
                m_valM = 0;
                for (int i = 0; i < 4; i++)
                {
                    m_valM <<= 8;
                    m_valM += mem[mem_addr + i];
                }
            }
            else
                m_valM = 0;
            if (mem_write)
            {
                for (int i = 0; i < 4; i++)
                    mem[mem_addr + i] = (byte)((M_valA >> (8 * i)) & 0xff);
            }
        }
        //Update the status
        m_stat = dmem_error ? SADR : M_stat;
    }
    #endregion
    #region Write back
    void Write_back()
    {
        //Set E port register ID
        w_dstE = W_dstE;
        //## Set E port value
        w_valE = W_valE;
        //Set M port register ID
        w_dstM = W_dstM;
        //Set M port value
        w_valM = W_valM;
        //Update processor status
        if (W_stat == SBUB)
            Stat = SAOK;
        else
            Stat = W_stat;
        if (Stat == SAOK)//only if stat is AOK can we update register file
        {
            if (W_dstE != RNONE)
                reg_file[W_dstE] = W_valE;
            if (W_dstM != RNONE)
                reg_file[W_dstM] = W_valM;
        }
    }
    #endregion
    #region Pipeline Register Control
    void Pipeline_Register_Control()
    {
        //Should I stall or inject a bubble into Pipeline Register F?
        //At most one of these can be true.
        F_bubble = false;
        if ((E_icode == IMRMOVL || E_icode == IPOPL) && (E_dstM == d_srcA || E_dstM == d_srcB) || (IRET == D_icode || IRET == E_icode || IRET == M_icode))
            F_stall = true;
        else
            F_stall = false;
        //Should I stall or inject a bubble into Pipeline Register D?
        //At most one of these can be true.
        if ((E_icode == IMRMOVL || E_icode == IPOPL) && (E_dstM == d_srcA || E_dstM == d_srcB))
            D_stall = true;
        else
            D_stall = false;
        if ((E_icode == IJXX && !e_Cnd) || !((E_icode == IMRMOVL || E_icode == IPOPL) && (E_dstM == d_srcA || E_dstM == d_srcB)) && (IRET == D_icode || IRET == E_icode || IRET == M_icode))
            D_bubble = true;
        else
            D_bubble = false;
        //Should I stall or inject a bubble into Pipeline Register E?
        //At most one of these can be true.
        E_stall = false;
        if ((E_icode == IJXX && !e_Cnd) || (E_icode == IMRMOVL || E_icode == IPOPL) && (E_dstM == d_srcA || E_dstM == d_srcB))
            E_bubble = true;
        else
            E_bubble = false;
        //Should I stall or inject a bubble into Pipeline Register M?
        //At most one of these can be true.
        M_stall = false;
        if (m_stat == SADR || m_stat == SINS || m_stat == SHLT || W_stat == SADR || W_stat == SINS || W_stat == SHLT)
            M_bubble = true;
        else
            M_bubble = false;
        //Should I stall or inject a bubble into Pipeline Register W?
        if (W_stat == SADR || W_stat == SINS || W_stat == SHLT)
            W_stall = true;
        else
            W_stall = false;
        W_bubble = false;
    }
    #endregion
    #region Clock Rise
    //pass value from Predict PC module to Fetch register
    void P2F()
    {
        if (!F_stall)
        {
            F_predPC = f_predPC;
        }
    }

    //pass value from fetch stage to Decode register
    void F2D()
    {
        if (!D_stall)
        {
            D_stat = f_stat; D_icode = f_icode; D_ifun = f_ifun; D_rA = f_rA; D_rB = f_rB; D_valC = f_valC; D_valP = f_valP;
        }
        if (D_bubble)
        {
            D_stat = SBUB; D_icode = INOP; D_ifun = FNONE; D_rA = RNONE; D_rB = RNONE; D_valC = 0; D_valP = 0;
        }
    }

    //pass value from decode stage to Execute register
    void D2E()
    {
        if (E_bubble)
        {
            E_stat = SBUB; E_icode = INOP; E_ifun = FNONE; E_valC = 0; E_valA = 0; E_valB = 0; E_dstE = RNONE; E_dstM = RNONE; E_srcA = RNONE; E_srcB = RNONE;
        }
        else
        {
            E_stat = d_stat; E_icode = d_icode; E_ifun = d_ifun; E_valC = d_valC; E_valA = d_valA; E_valB = d_valB; E_dstE = d_dstE; E_dstM = d_dstM; E_srcA = d_srcA; E_srcB = d_srcB;
        }
    }

    //pass value from execute stage to Memory register
    void E2M()
    {
        if (M_bubble)
        {
            M_stat = SBUB; M_icode = INOP; M_Cnd = false; M_valE = 0; M_valA = 0; M_dstE = RNONE; M_dstM = RNONE;
        }
        else
        {
            M_stat = e_stat; M_icode = e_icode; M_Cnd = e_Cnd; M_valE = e_valE; M_valA = e_valA; M_dstE = e_dstE; M_dstM = e_dstM;
        }
    }

    //pass value from memory stage to Write Back register
    void M2W()
    {
        if (!W_stall)
        {
            W_stat = m_stat; W_icode = m_icode; W_valE = m_valE; W_valM = m_valM; W_dstE = m_dstE; W_dstM = m_dstM;
        }
    }

    //start a new stage in a cycle
    void start_new_stage()
    {
        Write_back();
        Memory();
        Execute();
        Decode();
        Fetch();
    }
    /*Clock Rise
        * increase the cycle count
        * decide the pipeline register control signal
        * then pass the stage values to the pipeline registers
        */
    void clock_rise()
    {
        cycle_cnt++;
        start_new_stage();
        Pipeline_Register_Control();
        P2F();
        F2D();
        D2E();
        E2M();
        M2W();
    }
    public void runpipe(string filePath)
    {
        initial_Y86Pipeline();
        read(filePath);
        record_initial();
        record_save(0);
        while (Stat == SAOK)
        {
            clock_rise();
            record_save(cycle_cnt);
        }
        Debug.Log("cycle_cnt = " + cycle_cnt.ToString());
    }
    #endregion
    #region print
    #region Fetch Stage 
    void printFetch(StringWriter sw, int clk)
    {
        //Select PC module: What address should instruction be fetched at
        if (rM_icode[clk-1] == IJXX && !rM_Cnd[clk-1])
            sw.WriteLine("\tf_pc <- M_valA \t = 0x{0:x8}", rf_pc[clk]);
        else if (rW_icode[clk-1] == IRET)
            sw.WriteLine("\tf_pc <- W_valM \t = 0x{0:x8}", rf_pc[clk]); 
        else
            sw.WriteLine("\tf_pc <- F_predPC \t = 0x{0:x8}", rf_pc[clk]);
        //Instruction memory & Split & Align
        sw.WriteLine("\timem_icode \t = 0x{0:x}", rimem_icode[clk]);
        sw.WriteLine("\timem_ifun \t = 0x{0:x}", rimem_ifun[clk]);
        sw.WriteLine("\timem_error \t = {0}", Convert.ToString(rimem_error[clk]).ToLower());
        //Determine icode of fetched instruction
        if (rimem_error[clk])
            sw.WriteLine("\tf_icode <- INOP \t = 0x{0:x}", rf_icode[clk]); 
        else
            sw.WriteLine("\tf_icode <- imem_icode \t = 0x{0:x}", rf_icode[clk]);
        //Determine ifun
        if (rimem_error[clk])
            sw.WriteLine("\tf_ifun <- FNONE \t = 0x{0:x}", rf_ifun[clk]);
        else
            sw.WriteLine("\tf_ifun <- imem_ifun \t = 0x{0:x}", rf_ifun[clk]);
        //Is instruction valid?
        sw.WriteLine("\tinstr_valid \t = {0}", Convert.ToString(rinstr_valid[clk]).ToLower());
        //Does fetched instruction require a regid byte?
        sw.WriteLine("\tneed_regids \t = {0}", Convert.ToString(rneed_regids[clk]).ToLower());
        //Does fetched instruction require a constant word?
        sw.WriteLine("\tneed_valC \t = {0}", Convert.ToString(rneed_valC[clk]).ToLower());
        //Determine status code for fetched instruction
        if (rimem_error[clk])
            sw.WriteLine("\tf_stat = SADR;\t");
        else if (!rinstr_valid[clk])
            sw.WriteLine("\tf_stat = SINS;\t");
        else if (rf_icode[clk] == IHALT)
            sw.WriteLine("\tf_stat = SHLT;\t");
        else
            sw.WriteLine("\tf_stat = SAOK;\t");
        //fetch module
        sw.WriteLine("\tf_rA \t = 0x{0:x}", rf_rA[clk]);
        sw.WriteLine("\tf_rB \t = 0x{0:x}", rf_rB[clk]);
        sw.WriteLine("\tf_valC \t = 0x{0:x8}", rf_valC[clk]);
        //PC increment module
        sw.WriteLine("\tf_valP \t = 0x{0:x8}", rf_valP[clk]);
        //Predict next value of PC
        if (rf_icode[clk] == IJXX || rf_icode[clk] == ICALL)
            sw.WriteLine("\tf_predPC <- f_valC \t = 0x{0:x8}", rf_predPC[clk]);
        else
            sw.WriteLine("\tf_predPC <- f_valP \t = 0x{0:x8}", rf_predPC[clk]);
    }
    #endregion
    #region Decode Stage
    void printDecode(StringWriter sw, int clk)
    {
        sw.WriteLine("\td_stat <- D_stat \t = 0x{0:x}", rd_stat[clk]);
        sw.WriteLine("\td_icode <- D_icode \t = 0x{0:x}", rd_icode[clk]);
        sw.WriteLine("\td_ifun <- D_ifun \t = 0x{0:x}", rd_ifun[clk]);
        sw.WriteLine("\td_valC <- D_valC \t = 0x{0:x8}", rd_valC[clk]);
        //What register should be used as the A source?
        if (rD_icode[clk-1] == IRRMOVL || rD_icode[clk - 1] == IRMMOVL || rD_icode[clk - 1] == IOPL || rD_icode[clk - 1] == IPUSHL)
            sw.WriteLine("\td_srcA <- D_rA \t = 0x{0:x}", rd_srcA[clk]);
        else if (rD_icode[clk - 1] == IPOPL || rD_icode[clk - 1] == IRET)
            sw.WriteLine("\td_srcA <- RESP \t = 0x{0:x}", rd_srcA[clk]); 
        else
            sw.WriteLine("\td_srcA <- RNONE \t = 0x{0:x}", rd_srcA[clk]);//Don’t need register
        //What register should be used as the B source?
        if (rD_icode[clk - 1] == IOPL || rD_icode[clk - 1] == IRMMOVL || rD_icode[clk - 1] == IMRMOVL)
            sw.WriteLine("\td_srcB <- D_rB \t = 0x{0:x}", rd_srcB[clk]);
        else if (rD_icode[clk - 1] == IPUSHL || rD_icode[clk - 1] == IPOPL || rD_icode[clk - 1] == ICALL || rD_icode[clk - 1] == IRET)
            sw.WriteLine("\td_srcB <- RESP \t = 0x{0:x}", rd_srcB[clk]);
        else
            sw.WriteLine("\td_srcB <- RNONE \t = 0x{0:x}", rd_srcB[clk]);//Don’t need register
        //What register should be used as the E destination?
        if (rD_icode[clk - 1] == IRRMOVL || rD_icode[clk - 1] == IIRMOVL || rD_icode[clk - 1] == IOPL)
            sw.WriteLine("\td_dstE <- D_rB \t = 0x{0:x}", rd_srcB[clk]);
        else if (rD_icode[clk - 1] == IPUSHL || rD_icode[clk - 1] == IPOPL || rD_icode[clk - 1] == ICALL || rD_icode[clk - 1] == IRET)
            sw.WriteLine("\td_dstE <- RESP \t = 0x{0:x}", rd_srcB[clk]); 
        else
            sw.WriteLine("\td_dstE <- RNONE \t = 0x{0:x}", rd_srcB[clk]); // Don’t write any register
        //What register should be used as the M destination?
        if (rD_icode[clk - 1] == IMRMOVL || rD_icode[clk - 1] == IPOPL)
            sw.WriteLine("\td_dstM = D_rA \t = 0x{0:x}", rd_dstM[clk]);
        else
            sw.WriteLine("\td_dstM = RNONE \t = 0x{0:x}", rd_dstM[clk]); // Don’t write any register
        //What should be the A value?
        //Forward into decode stage for valA
        //Sel+Fwd A module
        sw.WriteLine(rForwarding_A[clk]);
        //Fwd B module
        sw.WriteLine(rForwarding_B[clk]);
    }
    #endregion
    #region Execute Stage
    void printExecute(StringWriter sw, int clk)
    {
        sw.WriteLine("\te_stat <- E_stat = " + stat[re_stat[clk]]);
        sw.WriteLine("\te_icode <- E_icode \t = 0x{0:x}", re_icode[clk]);
        //Select input A to ALU
        if (rE_icode[clk - 1] == IRRMOVL || rE_icode[clk - 1] == IOPL)
            sw.WriteLine("\taluA <- E_valA = \t = 0x{0:x8}", raluA[clk]);
        else if (rE_icode[clk - 1] == IRRMOVL || rE_icode[clk - 1] == IRMMOVL || rE_icode[clk - 1] == IMRMOVL)
            sw.WriteLine("\taluA <- E_valC = \t = 0x{0:x8}", raluA[clk]);
        else if (rE_icode[clk - 1] == ICALL || rE_icode[clk - 1] == IPUSHL)
            sw.WriteLine("\taluA = -4");
        else if (rE_icode[clk - 1] == IRET || rE_icode[clk - 1] == IPOPL)
            sw.WriteLine("\taluA = 4"); 
        else
            sw.WriteLine("\taluA = 0"); //Other instructions don’t need ALU
        //Select input B to ALU
        if (rE_icode[clk - 1] == IRMMOVL || rE_icode[clk - 1] == IMRMOVL || rE_icode[clk - 1] == IOPL || rE_icode[clk - 1] == ICALL || rE_icode[clk - 1] == IPUSHL || rE_icode[clk - 1] == IRET || rE_icode[clk - 1] == IPOPL)
            sw.WriteLine("\taluB <- E_valB = \t = 0x{0:x8}", raluB[clk]);
        else
            sw.WriteLine("\taluB = 0");
        //Set the ALU function
        if (rE_icode[clk - 1] == IOPL)
            sw.WriteLine("\talufun <- E_ifun = {0}", ralufun[clk]);
        else
            sw.WriteLine("\talufun = ALUADD");
        //ALU module
        switch (alufun)
        {
            case 0: sw.WriteLine("\te_valE <- aluB + aluA = 0x{0:x8}", re_valE[clk]);  break;
            case 1: sw.WriteLine("\te_valE <- aluB - aluA = 0x{0:x8}", re_valE[clk]); break;
            case 2: sw.WriteLine("\te_valE <- aluB & aluA = 0x{0:x8}", re_valE[clk]); break;
            case 3: sw.WriteLine("\te_valE <- aluB ^ aluA = 0x{0:x8}", re_valE[clk]); break;
            default: sw.WriteLine("e_valE = 0"); break;
        }
        //Should the condition codes be updated?
        //State changes only during normal operation
        sw.WriteLine("set_cc = {0}", Convert.ToString(rset_cc[clk]).ToLower());
        //Generate valA in execute stage
        sw.WriteLine("\te_valA <- E_valA = 0x{0:x8}", re_valA[clk]);
        //Set dstE to RNONE in event of not-taken conditional move
        if (rE_icode[clk - 1] == IRRMOVL && !e_Cnd)
            sw.WriteLine("\te_dstE = e_dstE = RNONE");
        else
            sw.WriteLine("\te_dstE <- E_dstE = 0x{0:x}", re_dstE[clk]);
        //cc
        if (rset_cc[clk])
        {
            sw.WriteLine("SF = {0}", Convert.ToString(rSF[clk]).ToLower());
            sw.WriteLine("ZF = {0}", Convert.ToString(rZF[clk]).ToLower());
            sw.WriteLine("OF = {0}", Convert.ToString(rOF[clk]).ToLower());
        }
        //e_Cnd module
        sw.WriteLine("e_Cnd = {0}", Convert.ToString(re_Cnd[clk]).ToLower());
        sw.WriteLine("\te_dstM <- E_dstM; = 0x{0:x}", re_dstM[clk]);
    }
    #endregion
    #region Memory Stage
    void printMemory(StringWriter sw, int clk)
    {
        sw.WriteLine("\tm_icode <- M_icode; \t = 0x{0:x}", rm_icode[clk]);
        sw.WriteLine("\tm_valE <- M_valE \t = 0x{0:x8}", rm_valE[clk]);
        sw.WriteLine("\tm_dstE <- M_dstE \t = 0x{0:x}", rm_dstE[clk]);
        sw.WriteLine("\tm_dstM <- M_dstM \t = 0x{0:x}", rm_dstM[clk]);
        //Select memory address
        if (rM_icode[clk - 1] == IRMMOVL || rM_icode[clk - 1] == IPUSHL || rM_icode[clk - 1] == ICALL || rM_icode[clk - 1] == IMRMOVL)
            sw.WriteLine("\tmem_addr <- M_valE \t = 0x{0:x8}", rmem_addr[clk]);
        else if (rM_icode[clk - 1] == IPOPL || rM_icode[clk - 1] == IRET)
            sw.WriteLine("\tmem_addr <- M_valA \t = 0x{0:x8}", rmem_addr[clk]);//Other instructions don’t need address
        //Set read control signal
        if (rM_icode[clk - 1] == IMRMOVL || rM_icode[clk - 1] == IPOPL || rM_icode[clk - 1] == IRET)
            sw.WriteLine("\tmem_read = true");
        else
            sw.WriteLine("\tmem_read = false");
        //Set write control signal
        if (rM_icode[clk - 1] == IRMMOVL || rM_icode[clk - 1] == IPUSHL || rM_icode[clk - 1] == ICALL)
            sw.WriteLine("\tmem_write = true");
        else
            sw.WriteLine("\tmem_write = false");
        //read / write memory
        if (rmem_addr[clk] > MAX_MEM_CAP)
            sw.WriteLine("\tdmem_error = true");
        else
        {
            sw.WriteLine("\tdmem_error = false");
            sw.WriteLine("\tm_valM = {0:x8}", rm_valM[clk]);
            if (rmem_write[clk])
                sw.WriteLine("\tM[0x{0:x8}] <- M_valA = {0:x8}", rmem_addr[clk], rM_valA[clk - 1]);
        }
        //Update the status
        sw.WriteLine("\tm_stat = " + stat[rm_stat[clk]]);
    }
    #endregion
    #region Write back
    void printWrite_back(StringWriter sw, int clk)
    {
        //Set E port register ID
        sw.WriteLine("\tw_dstE <- W_dstE \t = 0x{0:x}", rw_dstE[clk]);
        //## Set E port value
        sw.WriteLine("\tw_valE <- W_valE \t = 0x{0:x8}", rw_valE[clk]);
        //Set M port register ID
        sw.WriteLine("\tw_dstM <- W_dstM \t = 0x{0:x}", rw_dstM[clk]);
        //Set M port value
        sw.WriteLine("\tw_valM <- W_valM \t = 0x{0:x8}", rw_valM[clk]);
        //Update processor status
        if (rW_stat[clk - 1] == SBUB)
            sw.WriteLine("\tStat = SAOK");
        else
            sw.WriteLine("\tStat <- W_stat = " + stat[rStat[clk]]);
        if (rStat[clk] == SAOK)//only if stat is AOK can we update register file
        {
            if (rW_dstE[clk - 1] != RNONE)
                sw.WriteLine("\tR[" + reg[rW_dstE[clk - 1]] + "] <- W_valE = {0:x8}", rreg_file[rW_dstE[clk - 1],clk]);
            if (rW_dstM[clk - 1] != RNONE)
                sw.WriteLine("\tR[" + reg[rW_dstM[clk - 1]] + "] <- W_valM = {0:x8}", rreg_file[rW_dstM[clk - 1],clk]);
        }
    }
    #endregion

    #region Pipeline Register Control
    void printPipeline_Register_Control(StringWriter sw, int clk)
    {
        sw.WriteLine("\tF_bubble = {0}", Convert.ToString(rF_bubble[clk]).ToLower());
        sw.WriteLine("\tF_stall = {0}", Convert.ToString(rF_stall[clk]).ToLower());
        sw.WriteLine("\tD_bubble = {0}", Convert.ToString(rD_bubble[clk]).ToLower());
        sw.WriteLine("\tD_stall = {0}", Convert.ToString(rD_stall[clk]).ToLower());
        sw.WriteLine("\tE_bubble = {0}", Convert.ToString(rE_bubble[clk]).ToLower());
        sw.WriteLine("\tE_stall = {0}", Convert.ToString(rE_stall[clk]).ToLower());
        sw.WriteLine("\tM_bubble = {0}", Convert.ToString(rM_bubble[clk]).ToLower());
        sw.WriteLine("\tM_stall = {0}", Convert.ToString(rM_stall[clk]).ToLower());
        sw.WriteLine("\tW_bubble = {0}", Convert.ToString(rW_bubble[clk]).ToLower());
        sw.WriteLine("\tW_stall = {0}", Convert.ToString(rW_stall[clk]).ToLower());
    }
    #endregion
    #region Clock Rise
    //pass value from Predict PC module to Fetch register
    void printP2F(StringWriter sw, int clk)
    {
        if (!rF_stall[clk])
        {
            sw.WriteLine("\tF_predPC\t<- f_predPC =\t0x{0:x8}", rF_predPC[clk]);
        }
    }

    //pass value from fetch stage to Decode register
    void printF2D(StringWriter sw, int clk)
    {
        if (!rD_stall[clk])
        {
            sw.WriteLine("\tD_stat\t<- f_stat =\t0x{0:x8}", stat[rD_stat[clk]]);
            sw.WriteLine("\tD_icode\t<- f_icode =\t0x{0:x8}", rD_icode[clk]);
            sw.WriteLine("\tD_ifun\t<- f_ifun =\t0x{0:x8}", rD_ifun[clk]);
            sw.WriteLine("\tD_rA\t<- f_rA =\t0x{0:x8}", rD_rA[clk]);
            sw.WriteLine("\tD_rB\t<- f_rB =\t0x{0:x8}", rD_rB[clk]);
            sw.WriteLine("\tD_valC\t<- f_valC =\t0x{0:x8}", rD_valC[clk]);
            sw.WriteLine("\tD_valP\t<- f_valP =\t0x{0:x8}", rD_valP[clk]);
        }
        if (rD_bubble[clk])
        {
            sw.WriteLine("\tD_stat\t= SBUB");
            sw.WriteLine("\tD_icode\t= INOP");
            sw.WriteLine("\tD_ifun\t= FNONE");
            sw.WriteLine("\tD_rA\t= RNONE");
            sw.WriteLine("\tD_rB\t= RNONE");
            sw.WriteLine("\tD_valC\t= 0");
            sw.WriteLine("\tD_valP\t= 0");
        }
    }

    //pass value from decode stage to Execute register
    void printD2E(StringWriter sw, int clk)
    {
        if (rE_bubble[clk])
        {
            sw.WriteLine("\tE_stat\t = SBUB");
            sw.WriteLine("\tE_icode\t = INOP");
            sw.WriteLine("\tE_ifun\t = FNONE");
            sw.WriteLine("\tE_valC\t = 0");
            sw.WriteLine("\tE_valA\t = 0");
            sw.WriteLine("\tE_valB\t = 0");
            sw.WriteLine("\tE_dstE\t = RNON");
            sw.WriteLine("\tE_dstM\t = RNONE");
            sw.WriteLine("\tE_srcA\t = RNONE");
            sw.WriteLine("\tE_srcB\t = RNONE");
        }
        else
        {
            sw.WriteLine("\tE_stat\t<- d_stat =\t0x{0:x8}", stat[rE_stat[clk]]);
            sw.WriteLine("\tE_icode\t<- d_icode =\t0x{0:x8}", rE_icode[clk]);
            sw.WriteLine("\tE_ifun\t<- d_ifun =\t0x{0:x8}", rE_ifun[clk]);
            sw.WriteLine("\tE_valC\t<- d_valC =\t0x{0:x8}", rE_valC[clk]);
            sw.WriteLine("\tE_valA\t<- d_valA =\t0x{0:x8}", rE_valA[clk]);
            sw.WriteLine("\tE_valB\t<- d_valB =\t0x{0:x8}", rE_valB[clk]);
            sw.WriteLine("\tE_dstE\t<- d_dstE =\t0x{0:x8}", rE_dstE[clk]);
            sw.WriteLine("\tE_dstM\t<- d_dstM =\t0x{0:x8}", rE_dstM[clk]);
            sw.WriteLine("\tE_srcA\t<- d_srcA =\t0x{0:x8}", rE_srcA[clk]);
            sw.WriteLine("\tE_srcB\t<- d_srcB =\t0x{0:x8}", rE_srcB[clk]);
        }
    }

    //pass value from execute stage to Memory register
    void printE2M(StringWriter sw, int clk)
    {
        if (rM_bubble[clk])
        {
            sw.WriteLine("\tM_stat\t = SBUB");
            sw.WriteLine("\tM_icode\t = INOP");
            sw.WriteLine("\tM_Cnd\t = false");
            sw.WriteLine("\tM_valE\t = 0");
            sw.WriteLine("\tM_valA\t = 0");
            sw.WriteLine("\tM_dstE\t = RNONE");
            sw.WriteLine("\tM_dstM\t = RNONE");
        }
        else
        {
            sw.WriteLine("\tM_stat\t<- e_stat =\t0x{0:x8}", stat[rM_stat[clk]]);
            sw.WriteLine("\tM_icode\t<- e_icode =\t0x{0:x8}", rM_icode[clk]);
            sw.WriteLine("\tM_Cnd\t<- e_Cndn =\t0x{0:x8}", rM_Cnd[clk]);
            sw.WriteLine("\tM_valE\t<- e_valE =\t0x{0:x8}", rM_valE[clk]);
            sw.WriteLine("\tM_valA\t<- e_valA =\t0x{0:x8}", rM_valA[clk]);
            sw.WriteLine("\tM_dstE\t<- e_dstE =\t0x{0:x8}", rM_dstE[clk]);
            sw.WriteLine("\tM_dstM\t<- e_dstM =\t0x{0:x8}", rM_dstM[clk]);
        }
    }

    //pass value from memory stage to Write Back register
    void printM2W(StringWriter sw, int clk)
    {
        if (!rW_stall[clk])
        {
            sw.WriteLine("\tW_stat\t<- m_stat =\t0x{0:x8}", stat[rW_stat[clk]]);
            sw.WriteLine("\tW_icode\t<- m_icode =\t0x{0:x8}", rW_icode[clk]);
            sw.WriteLine("\tW_valE\t<- m_valE =\t0x{0:x8}", rW_valE[clk]);
            sw.WriteLine("\tW_valM\t<- m_valM =\t0x{0:x8}", rW_valM[clk]);
            sw.WriteLine("\tW_dstE\t<- m_dstE =\t0x{0:x8}", rW_dstE[clk]);
            sw.WriteLine("\tW_dstM\t<- m_dstM =\t0x{0:x8}", rW_dstM[clk]);
        }
    }
        
    #endregion
    #endregion
    #region output
    public string print_out(int i)
    {
        if (i == 0) return "";
        StringWriter sw = new StringWriter();
        sw.WriteLine("Cycle {0}", i);
        sw.WriteLine("\tStat\t = 0x{0:x8}", rStat[i]);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("reg:");
        sw.WriteLine("WRITE BACK:");
        printWrite_back(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("MEMORY:");
        printMemory(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("EXECUTE:");
        printExecute(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("DECODE:");
        printDecode(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("FETCH:");
        printFetch(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("Pipeline Register Control:");
        printPipeline_Register_Control(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("FETCH:");
        printP2F(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("DECODE:");
        printF2D(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("EXECUTE:");
        printD2E(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("MEMORY:");
        printE2M(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("WRITE BACK:");
        printM2W(sw, i);
        sw.WriteLine("--------------------------------");
        sw.WriteLine("");
        return sw.ToString();
    }
    #endregion
}

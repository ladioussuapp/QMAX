
using System;
using System.Collections;
using System.IO;

public class GeneticAlgorithm
{
    /// <summary>
    /// 染色体
    /// </summary>
    public class Chromosome
    {
        public byte[] Gene;

        public byte MaxValue;

        public Chromosome(int len, byte max)
        {
            Gene = new byte[len];
            Random r = new Random();
            for (int i = 0; i < len; i++)
            {
                Gene[i] = (byte)r.Next(max);
            }
        }

        protected Chromosome(Chromosome c)
        {
            this.Gene = c.Gene.Clone() as byte[];
        }


        public Chromosome Clone() { return new Chromosome(this); }
    }

    /// <summary>
    /// 种群规模
    /// </summary>
    public int M = 50;

    /// <summary>
    /// 交叉发生概率
    /// </summary>
    public float Pc = 0.02f;

    /// <summary>
    /// 变异发生概率
    /// </summary>
    public float Pm = 0.01f;

    /// <summary>
    /// 终止进化代数
    /// </summary>
    public int G = 100;

    /// <summary>
    /// 进化产生的任何一个个体的适应度函数超过Tf，则可以终止进化过程
    /// </summary>
    public int Tf = 100;

    /// <summary>
    /// 适应性函数
    /// </summary>
    public Func<Chromosome, int> CalcFitness;

    private Random random;

    public GeneticAlgorithm(Func<Chromosome, int> fitnessFunc,
                            int chromosomeLen,
                            int M,
                            float Pc,
                            float Pm,
                            int G,
                            int Tf)
    {
        this.M = M;
        this.Pc = Pc;
        this.Pm = Pm;
        this.G = G;
        this.Tf = Tf;
        this.CalcFitness = fitnessFunc;
        random = new Random();
    }

    public void Start()
    {
        //种群
        Chromosome[] population = new Chromosome[M];

        //代数
        int g = 0;
        //适应度
        int maxFitness = 0;
        while (g < G && maxFitness < Tf)
        {
            //计算种群Pop中每一个体的适应度
            int[] fitnessArray = new int[population.Length];
            for (int i = 0, n = population.Length; i < n; i++)
            {
                int f = CalcFitness(population[i]);
                fitnessArray[i] = f;
                maxFitness = f > maxFitness ? f : maxFitness;
            }

            Chromosome[] newPop = new Chromosome[M];

            int newPopCount = 0;
            while (newPopCount < M)
            {
                //根据适应度以比例选择算法从种群Pop中选出2个个体
                Chromosome chro1 = population[SelectChromosome(fitnessArray)].Clone();
                Chromosome chro2 = population[SelectChromosome(fitnessArray)].Clone();
                double pc = random.NextDouble();
                double pm = random.NextDouble();
                if (pc < Pc)
                {
                    //对2个个体按交叉概率Pc执行交叉操作
                    Crossover(chro1, chro2);
                }

                if (pm < Pm)
                {
                    //对2个个体按变异概率Pm执行变异操作
                    Mutation(chro1);
                    Mutation(chro2);
                }

                newPop[newPopCount] = chro1;
                newPopCount++;
                if (newPopCount < M)
                {
                    newPop[newPopCount] = chro2;
                    newPopCount++;
                }
            }

            population = newPop;
            g++;
        }
    }


    private int SelectChromosome(int[] fitnessArray)
    {
        //轮盘算法
        int m = 0;
        int totalWeight = 0;
        for (int i = 0, n = fitnessArray.Length; i < n; i++)
        {
            totalWeight += fitnessArray[i];
        }
        double p = random.Next(totalWeight);

        for (int i = 1, n = fitnessArray.Length; i < n; i++)
        {
            m += fitnessArray[i];
            if (p <= m)
                return i;
        }
        return 0;
    }



    /// <summary>
    /// 染色体交叉
    /// </summary>
    /// <param name="chro1"></param>
    /// <param name="chro2"></param>
    private void Crossover(Chromosome chro1, Chromosome chro2)
    {
        //单点杂交
        int idx = random.Next(chro1.Gene.Length);
        for (int i = idx, n = chro1.Gene.Length; i < n; i++)
        {
            byte t = chro1.Gene[i];
            chro1.Gene[i] = chro2.Gene[i];
            chro2.Gene[i] = t;
        }
    }


    /// <summary>
    /// 染色体突变
    /// </summary>
    /// <param name="chro"></param>
    private void Mutation(Chromosome chro)
    {
        //int mutationPoint = random.Next(chro.Gene.Length);
        //byte randomValue = (byte)random.Next(chro.MaxValue);
    }
}

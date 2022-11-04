using System;
using System.Collections.Generic;
namespace Table.Data
{
    [System.Serializable]
    public class TestJson
    {
        public List<Normal> Normal;
        public List<Elites> Elites;
        public List<Boss> Boss;
    }
    [System.Serializable]
    public class Normal
    {
        public int id;
        public int attackTime;
        public int attack;
        public float fireRate;
        public int life;
        public int speed;
        public int trajectoryID;
    }
    [System.Serializable]
    public class Elites
    {
        public int id;
        public int attackTime;
        public int attack;
        public float fireRate;
        public int life;
        public int speed;
        public int trajectoryID;
    }
    [System.Serializable]
    public class Boss
    {
        public int id;
        public int attackTime;
        public int attack;
        public float fireRate;
        public int life;
        public int speed;
        public int trajectoryID;
    }
}


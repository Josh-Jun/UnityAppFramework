using System;
using System.Collections.Generic;
namespace TableData
{
    public class Test
    {
        public List<Normal> Normal {get;set;}
        public List<Elites> Elites {get;set;}
        public List<Boss> Boss {get;set;}
    }
    public class Normal
    {
        public int id {get;set;}
        public int attackTime {get;set;}
        public int attack {get;set;}
        public float fireRate {get;set;}
        public int life {get;set;}
        public int speed {get;set;}
        public int trajectoryID {get;set;}
    }
    public class Elites
    {
        public int id {get;set;}
        public int attackTime {get;set;}
        public int attack {get;set;}
        public float fireRate {get;set;}
        public int life {get;set;}
        public int speed {get;set;}
        public int trajectoryID {get;set;}
    }
    public class Boss
    {
        public int id {get;set;}
        public int attackTime {get;set;}
        public int attack {get;set;}
        public float fireRate {get;set;}
        public int life {get;set;}
        public int speed {get;set;}
        public int trajectoryID {get;set;}
    }
}


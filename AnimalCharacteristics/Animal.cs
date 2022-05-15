using System;
using System.ComponentModel;

namespace AnimalCharacteristics
{
    [Serializable]
    public class Animal : INotifyPropertyChanged
    {
        public enum Continent
        { Европа, Азия, Америка, Африка, Австралия, Антарктида, none }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        [Browsable(false)]
        string name;
        [Browsable(false)]
        Continent area;
        [Browsable(false)]
        double population;
        [Browsable(false)]
        double avg_weight;
        [Browsable(false)]
        string picture;
        [DisplayName("Название")]
        public string Name { get => name; set => name = value; }
        [DisplayName("Обитание")]
        public Continent Area { get => area; set => area = value; }
        [DisplayName("Популяция")]
        [Description("Примерная численность вида на сегодняшний день")]
        public double Population { get => population; set => population = value; }
        [DisplayName("Средний вес (кг)")]
        public double AvgWeight { get => avg_weight; set => avg_weight = value; }
        [DisplayName("Изображение")]
        public string Pic
        {
            get => picture;
            set
            {
                if (picture != value)
                {
                    picture = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Pic"));
                }
            }
        }
        public Animal(string name, Continent area, double population, double avgWeight, string file)
        {
            Name = name;
            Area = area;
            Population = population;
            AvgWeight = avgWeight;
            Pic = file;
        }
        public Animal(string name, Continent area, double population, double avgWeight)
        {
            Name = name;
            Area = area;
            Population = population;
            AvgWeight = avgWeight;
        }
        public Animal()
        {
            Name = " ";
            Area = Continent.none;
            Population = 0;
            AvgWeight = 0;
        }

    }
}

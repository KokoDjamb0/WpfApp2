using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Client.Models
{
    public class Student
    {
        
        public int studentID { get; set; }

        public string First_Name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string FullName => $"{First_Name} {last_name}";
        public DateTime birthday_date { get; set; }
        public int educationID { get; set; }
        public int compositionID { get; set; }
        public string LivingAddress { get; set; }
        public DateTime Registration_start_date { get; set; }

        public string EducationInstitutionName { get; set; }
        public string CompositionName { get; set; }
    }
    public class grade

    {
        [Key]
        public int studentID { get; set; }
        public string Maths_grades { get; set; }
        public string Russian_language { get; set; }
        public string litra { get; set; }
        public string fizra { get; set; }
        public string it { get; set; }
        public string izo { get; set; }
        public string technologia { get; set; }
        public string obsh { get; set; }
        public string english_lang { get; set; }
        public string history { get; set; }
        public string StudentFullName { get; set; }
    }
    public class Education
    {
        
        public int educationID { get; set; }

        public string name_of_education_institution { get; set; }
        public string address_of_educational_institution { get; set; }
        public DateTime endDate { get; set; }

    }

    public class Parents
    {
        
        public int parentID { get; set; }
        public int studentID { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Middle_Name { get; set; }
        public string jobPlace { get; set; }
        public string job_Title { get; set; }
        public string Work_phone { get; set; }
        public string Living_adress { get; set; }

    }

    public class Composition_Family
    {
        
        public int compositionID { get; set; }

        public string family_completeness { get; set; }
    }

    public class creativity_activity
    {

        public int creativityID { get; set; }

        public int type_of_creativity { get; set; }
        public string name_of_creativity { get; set; }




        public Student Student { get; set; }

    }

    public class Eventss
    {
        
        public int eventID { get; set; }
        public string name_of_event { get; set; }
    }
    public class Participation_in_event

    {

        public int studentID { get; set; }
        public int eventID { get; set; }
    

        // Reference to Student
        public Student Student { get; set; }

        // Reference to Event
        public Eventss Eventss { get; set; }
    }

    public class PROMPUN
    {
        [Key]
        public int promotionID { get; set; }
        public string name_of_promotion { get; set; }
        public string name_of_punishment { get; set; }
        public int studentID { get; set; }
        public Student Student { get; set; }
    }

    public class Punishment
    {
        
        public int punishmentID { get; set; }
        public string name_of_punishment { get; set; }
    }
    public class Student_Punishments
    {
        
        public int studentID { get; set; }
        public int punishmentID { get; set; }
        public DateTime datee { get; set; }
    }
public class Student_Creativity
{
    public int student_creativity { get; set; }
    public int studentID { get; set; }
    public int creativityID { get; set; }
    public Student Student { get; set; }
    public creativity_activity creativity_activity { get; set; }
}
}


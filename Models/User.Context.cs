﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PaperPlanner_Application.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class PaperPlannerDBEntities : DbContext
    {
        public PaperPlannerDBEntities()
            : base("name=PaperPlannerDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<QuestionPaper> QuestionPapers { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<ApprovedQuestionPaper> ApprovedQuestionPapers { get; set; }
        public virtual DbSet<AnswersTable> AnswersTables { get; set; }
        public virtual DbSet<QuesPaper_Attempted_By_Users> QuesPaper_Attempted_By_Users { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace NerdDinner.Models
{
    public interface INerdDinners
    {
        Database Database { get; }
        int SaveChanges();
        DbEntityEntry Entry(object entity);

        DbSet<Dinner> Dinners { get; set; }
        DbSet<Models.Event> Events { get; set; }
    }

    public class NerdDinners : DbContext, INerdDinners
    {
        public NerdDinners() 
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<Dinner> Dinners { get; set; }
        
        public DbSet<Event> Events { get; set; }
		public DbSet<PopularDinner>  PopularDinners{get;set;}

        readonly static List<Action<NerdDinners,Event>> _eventPublishedHandlers = new List<Action<NerdDinners,Event>>();

        public static void OnEventsPublished(Action<NerdDinners,Event> handler) {
            lock(_eventPublishedHandlers) {
                _eventPublishedHandlers.Add(handler);
            }
        }

        public static void ClearEventHandlers() {
            _eventPublishedHandlers.Clear();
        }

        internal void StoreEvents(ICollection<Event> events) {
            foreach (var publishedEvent in events) {
                Events.Add(publishedEvent);
            }
            foreach(var handler in _eventPublishedHandlers) {
                foreach(var @event in events) {
                    handler(this,@event);
                }
            }
        }
    }

    public class CreateDatabaseIfNotExistsIncludingIndicesAndUDFs : CreateDatabaseIfNotExists<NerdDinners>
    {
        protected override void Seed(NerdDinners context) {
            base.Seed(context);
            context.Database.ExecuteSqlCommand("CREATE UNIQUE NONCLUSTERED INDEX IX_UQ_AggergateId_AggregateEventSequence ON dbo.Events ( AggregateId, AggregateEventSequence ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
            context.Database.ExecuteSqlCommand("CREATE UNIQUE NONCLUSTERED INDEX IX_UQ_DinnerGuid ON dbo.Dinners ( DinnerGuid ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
            context.Database.ExecuteSqlCommand("CREATE FUNCTION [dbo].[DistanceBetween] (@Lat1 as real, @Long1 as real, @Lat2 as real, @Long2 as real) RETURNS real AS BEGIN DECLARE @dLat1InRad as float(53); SET @dLat1InRad = @Lat1 * (PI()/180.0); DECLARE @dLong1InRad as float(53); SET @dLong1InRad = @Long1 * (PI()/180.0); DECLARE @dLat2InRad as float(53); SET @dLat2InRad = @Lat2 * (PI()/180.0); DECLARE @dLong2InRad as float(53); SET @dLong2InRad = @Long2 * (PI()/180.0);  DECLARE @dLongitude as float(53); SET @dLongitude = @dLong2InRad - @dLong1InRad; DECLARE @dLatitude as float(53); SET @dLatitude = @dLat2InRad - @dLat1InRad; /* Intermediate result a. */ DECLARE @a as float(53); SET @a = SQUARE (SIN (@dLatitude / 2.0)) + COS (@dLat1InRad) * COS (@dLat2InRad) * SQUARE(SIN (@dLongitude / 2.0)); /* Intermediate result c (great circle distance in Radians). */ DECLARE @c as real; SET @c = 2.0 * ATN2 (SQRT (@a), SQRT (1.0 - @a)); DECLARE @kEarthRadius as real; /* SET kEarthRadius = 3956.0 miles */ SET @kEarthRadius = 6376.5;        /* kms */  DECLARE @dDistance as real; SET @dDistance = @kEarthRadius * @c; return (@dDistance); END");
        }
    }

}
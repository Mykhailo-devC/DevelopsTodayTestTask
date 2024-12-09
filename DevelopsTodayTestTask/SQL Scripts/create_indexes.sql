CREATE INDEX IDX_PULocationID ON import_integration(PULocationID);
CREATE INDEX IDX_TipAmount_PULocationID ON import_integration(PULocationID, tip_amount);
CREATE INDEX IDX_TripDistance ON import_integration(trip_distance);
CREATE INDEX IDX_TimeTravel ON import_integration(tpep_pickup_datetime, tpep_dropoff_datetime);
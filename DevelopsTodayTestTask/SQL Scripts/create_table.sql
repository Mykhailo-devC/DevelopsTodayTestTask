CREATE TABLE import_integration
(
    tpep_pickup_datetime DATETIME NOT NULL,
    tpep_dropoff_datetime DATETIME NOT NULL,
    passenger_count INT NULL,
    trip_distance REAL NULL,
    store_and_fwd_flag VARCHAR(3) NULL,
    PULocationID INT NOT NULL,
    DOLocationID INT NOT NULL,
    fare_amount REAL NOT NULL,
    tip_amount REAL NOT NULL
);

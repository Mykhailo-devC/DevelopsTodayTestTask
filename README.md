# DevelopsTodayTestTask
To run this application you should use a terminal. Go to the root directory, run .exe file and specify the import file name, as first parameter, like:
`DevelopsTodayTestTask.exe <your_import_file_name.csv>`

## Potential Improvements
- **Resilency:** For now, if application detects error on any stage of processing, it will stop on the failed stage and revert it. It would be better to provide resilency, to handle some particulart cases, and prevent exiting the whole flow due to several errors. But this information should be discussed with customer.
- **Primary Key:** Add primary key to the table would be a huge improvement of db efficiency. And I expect fields `pickup_datetime`, `dropoff_datetime`, and `passenger_count` to be a composite primary key. But after I found that `passenger_count` can be `NULL`, I decline this idea. Maybe it is a good idea just add `id` field with auto increment. Also this will give an ability to use DbSet operations in entity framework, instead direct query execution, which are more flexible.
- **Alternative way of optimization:** Other way to optimize csv parsing or bulk insertion is TPL. But i think for large file it's better to operate with batch size, rather that working theads amount.

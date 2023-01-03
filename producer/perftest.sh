#!/bin/bash
# $1: broker list. for example: 10.4.0.7:9092,10.4.0.6:9092,10.4.0.5:9092,10.4.0.4:9092
# $2: kafka topic name. for example: largemessage
# $3: loop count
# sample command: ./perftest.sh 10.4.0.7:9092,10.4.0.6:9092,10.4.0.5:9092,10.4.0.4:9092 largemessage 10
n=$3
for (( i=1 ; i<=$n ; i++ )); 
do
    dotnet run --   $1 $2 $(($RANDOM%9000)) $(($RANDOM%100))
#sleep for a while
    sleep $(($RANDOM%10))
done
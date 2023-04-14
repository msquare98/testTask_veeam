# testTask

- This repo has original program to kill a process and log the the process killed data in testTask folder
- Another folder testtaks.Tests is where some possible unitTests has been written which should pass the original program

ProcessMonitor.cs is the program which is responsible for the given task

-This repo runs better on VS studio, so better it would be to download ot clone it to VS studio and build it.
-To run ProcessMonitor.cs, right click on the testTask and open a terminal or powershell and run the following command

    csc ProcessMonitor.cs  or csc ./ProcessMonitor.cs
    
one of the above command should run the ProcessMonitor.cs and generates app file ProcessMonitor.exe.

As mentioned in the task, now you can run in the terminal or powershell the following command to monitor and kill a process


     Eg: csc ProcessMonitor.exe notepad 2 1 or csc ./ProcessMonitor.exe notepad 2 1
     
 The above command is for a already opened notepad which is being monitored with a frequency of 1 miunute for a maximum of 2 minutes.
 After the maximum minitor time of 2 minutes, the process notepad will be killed and a log file will be generated.
 
 ProcessMonitor.exe and respective logfile were already included from the previous run just for an example.
 
 
 # testTask.Tests

There are a total of 6 tests written using Nunit framework

                 Monitor_ProcessExistsAndMaxLifetimeExceeded_KillsProcessAndLogs
                 
For this test to pass, a process(notepad) should already be opened an exceeded its lifetime
which is when the process will be killed instantly and a logfile will be generated in the directory.
    
                 Monitor_ProcessDoesNotExist_DoesNotKillProcess
                 
checks for a prcoess when there is none, doesn't kill anything or doesn't log anything

                Monitor_ProcessExistsAndMaxLifetimeNotExceeded_DoesNotKillProcess    
                
Starts a new process and checks for the runtime when not exceeded, it doesn't kill the process and keeps on running.
    
                 Monitor_ProcessIsCancelled_StopsProcessMonitor
                 
 Checks whether the process will be killed when a cancellation token is given
     
                 Monitor_ProcessExistsAndMaxLifetimeNotExceeded_DoesNotKillProcessOrLog
                 
Process already exists, checks for lifetime, when not exceeds, doesn't kills or logs
    
                 Monitor_ProcessNotFound_LogsProcessNotFound
                 
Checks for the process, runs for the maximumlifetime when no process i found, stops monitoring but not logs anything








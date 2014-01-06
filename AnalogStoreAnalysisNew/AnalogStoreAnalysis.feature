Feature: AnalogStoreAnalysis
         To check market Analysis 
             
                                     
 Background: 
  Given alteryx running at "http://gallery.alteryx.com/"
  And I am logged in using "deepak.manoharan@accionlabs.com" and "P@ssw0rd"
  
Scenario Outline: publish and run Analog Store Analysis
When I run analog store analysis with Address <Address> City <City> State <State> Zip <Zip> MaxSearchDistance <Maximum Search Distance>
##And I also enter the maximum search distance <Max Search Distance Miles>
Then I see the average sales result <Average Sales>

Examples: 
| Address           | City    | State | Zip   | Maximum Search Distance         | Average Sales |
| 1060 W ADDISON ST | CHICAGO | IL    | 60613 | 200                             | 149326       |

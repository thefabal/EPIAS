using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EPIAS {
    class Program {
        private static csettings settings = new csettings();

        static void Main( string[ ] args ) {
            settings.Get();
            settings.Set();

            if( args.Length > 0 ) {
                if( args[ 0 ] == "get" ) {
                    settings.Show();
                } else if( args[ 0 ] == "set" ) {
                    settings.Set();
                }
            }

            if( settings.Check() == false ) {
                settings.Set();
            }

            epias epias = new epias( true ) {
                user_name = settings.epias_username,
                user_pass = settings.epias_userpass,
                insane_mode = true
            };

            int opt = 0;
            string[ ] menu = {
                "List Meters whose supplier has changed", //1
                "List Deducted Meters Service", //2
                "List Meter Counts", //3
                "Meter EIC Querying Service", //4
                "List Meter Eic List Service", //5
                "Metering Point Listing Service", //6
                "List New Metering Points To Be Read Service", //7
                "List Non-obligatory To Read Metering Points Service", //8
                "Service to control meter is read or not and Listing past meters" //9
            };

            while( true ) {
                Console.Clear();
                for(int i = 0; i < menu.Length; i++ ) {
                    Console.WriteLine( " " + (i+1) + ". " + menu[i] );
                }
                Console.WriteLine( "Please select a function..." );

                string fn = Console.ReadLine();
                try {
                    opt = Convert.ToInt32( fn );
                    if( opt < 1 || opt > (menu.Length + 2) ) {
                        Console.WriteLine( "Wrong number. Please try again." );
                    }

                    break;
                } catch {
                    Console.WriteLine( "Wrong number. Please try again." );
                }
                Console.ReadLine();
            }

            switch( opt ) {
                case 1:
                    /**
                     * List Meters whose supplier has changed
                     **/
                    try {
                        List<ChangedSupplierMeterResponse> response = epias.GetChangedSupplierMeters( 
                            new DateTime( 2018, 11, 1 ), 
                            "EXACT_LIST" 
                        );

                        Console.WriteLine( "Total entry : " + response.Count );

                        using( StreamWriter sw = new StreamWriter( "GetChangedSupplierMeters_" + DateTime.Now.ToString( "yyyyMMdd_HHmm" ) + ".txt" ) ) {
                            foreach( ChangedSupplierMeterResponse item in response ) {
                                sw.Write( item.newMeterId + "," );
                                sw.Write( item.newMeterEic + "," );
                                sw.Write( item.newOrganizationEic + "," );
                                sw.Write( item.oldOrganizationEic + "," );
                                sw.Write( item.newCustomerNo + "," );
                                sw.Write( item.newMeterName + "," );
                                sw.Write( item.newMeterAddress + "," );
                                sw.Write( item.newMeterCountyId + "," );
                                sw.Write( item.newMeterReadingType + "," );
                                sw.Write( item.newMeterReadingTypeEnum + "," );
                                sw.Write( item.newProfileSubscriptionGroup + "," );
                                sw.Write( item.newAverageAnnualConsumption + "," );
                                sw.Write( item.newDistributionMeterCode + "," );
                                sw.Write( item.newprofileSubscriptionGroupName + "," );
                                sw.Write( item.newCity + "," );
                                sw.Write( item.oldOrganizationCode + "," );
                                sw.Write( item.newOrganizationCode );
                                sw.WriteLine( "" );
                            }
                        }
                    } catch( EXISTException ex ) {
                        Console.WriteLine( ex.error.resultCode );
                        Console.WriteLine( ex.error.resultDescription );
                        Console.WriteLine( ex.error.resultType );
                    } catch( Exception ex ) {
                        Console.WriteLine( ex.Message );
                    }

                    break;

                case 2:
                    /**
                     * List Deducted Meters Service 
                     **/
                    try {
                        List<DeductedMeterResponse> response = epias.GetDeductedMeters( 
                            new DateTime( 2018, 11, 1 ) 
                        );

                        Console.WriteLine( "Total entry : " + response.Count );

                        using( StreamWriter sw = new StreamWriter( "GetDeductedMeters_" + DateTime.Now.ToString( "yyyyMMdd_HHmm" ) + ".txt" ) ) {
                            foreach( DeductedMeterResponse item in response ) {
                                sw.Write( item.meterId + "," );
                                sw.Write( item.meterEffectiveDate + "," );
                                sw.Write( item.meterEic + "," );
                                sw.Write( item.city + "," );
                                sw.Write( item.meterSerialNo + "," );
                                sw.Write( item.customerNo + "," );
                                sw.Write( item.meterName + "," );
                                sw.Write( item.settlementPointId + "," );
                                sw.Write( item.settlementPointName + "," );
                                sw.WriteLine( "" );
                            }
                        }
                    } catch( EXISTException ex ) {
                        Console.WriteLine( ex.error.resultCode );
                        Console.WriteLine( ex.error.resultDescription );
                        Console.WriteLine( ex.error.resultType );
                    } catch( Exception ex ) {
                        Console.WriteLine( ex.Message );
                    }
                    break;

                case 3:
                    /**
                     * List Meter Counts
                     **/
                    try {
                        List<meterCountResponseList> response = epias.GetMeterCount( 
                            new DateTime( 2018, 11, 1 ), 
                            "PORTFOLIO" 
                        );

                        Console.WriteLine( "Total entry : " + response.Count );

                        using( StreamWriter sw = new StreamWriter( "GetMeterCount_" + DateTime.Now.ToString( "yyyyMMdd_HHmm" ) + ".txt" ) ) {
                            foreach( meterCountResponseList item in response ) {
                                sw.Write( item.meterEffectiveDate + "," );
                                sw.Write( item.readingType + "," );
                                sw.Write( item.meterCount );
                                sw.WriteLine( "" );
                            }
                        }
                    } catch( EXISTException ex ) {
                        Console.WriteLine( ex.error.resultCode );
                        Console.WriteLine( ex.error.resultDescription );
                        Console.WriteLine( ex.error.resultType );
                    } catch( Exception ex ) {
                        Console.WriteLine( ex.Message );
                    }

                    break;

                case 4:
                    /**
                     * Meter EIC Querying Service
                     **/
                    try {
                        List<MeteringPointEICQueryResponseData> response = epias.MeteringPointEICQuery( 
                            new List<MeteringPointEICQuery>() {
                                new MeteringPointEICQuery() {
                                    meterEic = ""
                                }
                            }
                        );

                        Console.WriteLine( "Total entry : " + response.Count );

                        using( StreamWriter sw = new StreamWriter( "MeteringPointEICQuery_" + DateTime.Now.ToString( "yyyyMMdd_HHmm" ) + ".txt" ) ) {
                            foreach( MeteringPointEICQueryResponseData item in response ) {
                                sw.Write( item.meterEic + "," );
                                sw.Write( item.distributionMeterId + "," );
                                sw.Write( item.customerNo + "," );
                                sw.Write( item.eligibleConsumptionType + "," );
                                sw.Write( item.meterUsageState + "," );
                                sw.Write( item.meteringPointName );
                                sw.Write( item.meteringAddress + "," );
                                sw.Write( item.cityId + "," );
                                sw.Write( item.countyId + "," );
                                sw.Write( item.meterReadingCompanyId + "," );
                                sw.Write( item.meterReadingCompanyEic + "," );
                                sw.Write( item.status + "," );
                                sw.Write( item.description + "," );
                                sw.Write( item.countyId );
                                sw.WriteLine( "" );
                            }
                        }
                    } catch( EXISTException ex ) {
                        Console.WriteLine( ex.error.resultCode );
                        Console.WriteLine( ex.error.resultDescription );
                        Console.WriteLine( ex.error.resultType );
                    } catch( Exception ex ) {
                        Console.WriteLine( ex.Message );
                    }

                    break;

                case 5:
                    /**
                     * List Meter Eic List Service
                     **/
                    try {
                        List<MeterEicInfoResponse> response = epias.GetMeterEic( 
                            new DateTime( 2018, 11, 1 ) 
                        );

                        Console.WriteLine( "Total entry : " + response.Count );

                        using( StreamWriter sw = new StreamWriter( "GetMeterEicRequest_" + DateTime.Now.ToString( "yyyyMMdd_HHmm" ) + ".txt" ) ) {
                            foreach( MeterEicInfoResponse item in response ) {
                                sw.Write( item.meterEic + "," );
                                sw.Write( item.readingType + "," );
                                sw.Write( item.meterId );
                                sw.WriteLine( "" );
                            }
                        }
                    } catch( EXISTException ex ) {
                        Console.WriteLine( ex.error.resultCode );
                        Console.WriteLine( ex.error.resultDescription );
                        Console.WriteLine( ex.error.resultType );
                    } catch( Exception ex ) {
                        Console.WriteLine( ex.Message );
                    }

                    break;

                case 6:
                    /**
                     * Metering Point Listing Service
                     **/
                    try {
                        List<MeteringPointResponse> response = epias.GetMeteringPoints(
                            new ListMeteringPointsRequest() {
                                meterEIC = "",
                                meterUsageState = "IN_USE",
                                eligibleConsumptionType = "ELIGIBLE_CONSUMER",
                                supplierType = "END_USE_SUPPLIER",
                                meterEffectiveDate = new DateTime( 2018, 11, 1 )
                            }
                        );

                        using( StreamWriter sw = new StreamWriter( "GetMeteringPoints_" + DateTime.Now.ToString( "yyyyMMdd_HHmm" ) + ".txt" ) ) {
                            foreach( MeteringPointResponse item in response ) {
                                sw.Write( item.id + "," );
                                sw.Write( item.meterEic + "," );
                                sw.Write( item.distributionMeterId + "," );
                                sw.Write( item.meterReadingCompany + "," );
                                sw.Write( item.withDrawalDeducSettlementPointEic + "," );
                                sw.Write( item.meterName );
                                sw.Write( item.meterAddress + "," );
                                sw.Write( item.countyId + "," );
                                sw.Write( item.transformerInputVoltage + "," );
                                sw.Write( item.transformerOutputVoltage + "," );
                                sw.Write( item.customerNo + "," );
                                sw.Write( item.substation + "," );
                                sw.Write( item.connectionPointLocation + "," );
                                sw.Write( item.connectionPointLocationOther + "," );
                                sw.Write( item.meteringVoltage + "," );
                                sw.Write( item.profileType + "," );
                                sw.Write( item.profileSubscriptionGroup + "," );
                                sw.Write( item.readingType + "," );
                                sw.Write( item.transformerPower + "," );
                                sw.Write( item.busbarVoltage + "," );
                                sw.Write( item.noLoadLoss + "," );
                                sw.Write( item.loadLoss + "," );
                                sw.Write( item.temperatureCoefficient + "," );
                                sw.Write( item.lineLength + "," );
                                sw.Write( item.lineSection + "," );
                                sw.Write( item.lineCircuit + "," );
                                sw.Write( item.transLossFactorStatus + "," );
                                sw.Write( item.averageAnnualConsumption + "," );
                                sw.Write( item.supplyPosition + "," );
                                sw.Write( item.withdrawalPosition + "," );
                                sw.Write( item.addressCode + "," );
                                sw.Write( item.zonningPosition + "," );
                                sw.Write( item.usageState + "," );
                                sw.Write( item.organizedIndustrialZoneEic + "," );
                                sw.Write( item.canLoadProfile + "," );
                                sw.Write( item.mainEligibleConsumptionEic + "," );
                                sw.Write( item.supplyDeductionSettlementPoint + "," );
                                sw.Write( item.eligibleConsumptionType + "," );
                                sw.Write( item.amr + "," );
                                sw.Write( item.maxAnnualConsumption + "," );
                                sw.Write( item.estimation + "," );
                                sw.Write( item.withdrawalPositionDescription + "," );
                                sw.Write( item.serialNumber + "," );
                                sw.Write( item.manufacturer + "," );
                                sw.Write( item.supplierOrganization + "," );
                                sw.Write( item.contractPower + "," );
                                sw.Write( item.tariffClass + "," );
                                sw.Write( item.mainTariffGroup + "," );
                                sw.Write( item.activityCode + "," );
                                sw.Write( item.meteringType + "," );
                                sw.Write( item.countyName + "," );
                                sw.Write( item.organizedIndustrialZone + "," );
                                sw.Write( item.meteringVoltageValue + "," );
                                sw.Write( item.busbarVoltageValue + "," );
                                sw.Write( item.lineCircuitDesc + "," );
                                sw.Write( item.temperatureCoefficientValue + "," );
                                sw.Write( item.profileSubscriptionGroupDesc + "," );
                                sw.Write( item.profileTypeDesc + "," );
                                sw.Write( item.withdrawalPositionName + "," );
                                sw.Write( item.supplyPositionName + "," );
                                sw.Write( item.eligibleConsumptionTypeDesc + "," );
                                sw.Write( item.usageStateDesc + "," );
                                sw.Write( item.supplierTypeDesc + "," );
                                sw.Write( item.connectionPointLocationDesc + "," );
                                sw.Write( item.substationDesc + "," );
                                sw.Write( item.registrationDate );
                                sw.WriteLine( "" );
                            }
                        }
                    } catch( EXISTException ex ) {
                        Console.WriteLine( ex.error.resultCode );
                        Console.WriteLine( ex.error.resultDescription );
                        Console.WriteLine( ex.error.resultType );
                    } catch( Exception ex ) {
                        Console.WriteLine( ex.Message );
                    }

                    break;

                case 7:
                    /**
                     * List New Metering Points To Be Read Service
                     **/
                    try {
                        List<ReadingMeteringPointResponse> response = epias.GetNewMeteringPoints( 
                            new ListNewMeteringPointsToBeRead() {
                                term = new DateTime( 2018, 11, 1 ),
                                listType = "EXACT_LIST",
                                range = new Range() {
                                    begin = 0,
                                    end = 1
                                }
                            } 
                        );

                        using( StreamWriter sw = new StreamWriter( "ReadingMeteringPoint_" + DateTime.Now.ToString( "yyyyMMdd_HHmm" ) + ".txt" ) ) {
                            foreach( ReadingMeteringPointResponse item in response ) {
                                sw.Write( item.meterEic + "," );
                                sw.Write( item.meterName + "," );
                                sw.Write( item.meterId + "," );
                                sw.Write( item.meterEffectiveDate + "," );
                                sw.Write( item.readingType + "," );
                                sw.Write( item.readingTypeId + "," );
                                sw.Write( item.meterLossesType + "," );
                                sw.Write( item.organization + "," );
                                sw.Write( item.meterReadingOrganization + "," );
                                sw.Write( item.profileSubscriptionGroup + "," );
                                sw.Write( item.distributionMeterCode + "," );
                                sw.Write( item.customerNo + "," );
                                sw.Write( item.meterAddress + "," );
                                sw.Write( item.countyId + "," );
                                sw.Write( item.averageAnnualConsumption + "," );
                                sw.Write( item.organizationEic + "," );
                                sw.Write( item.organizationCode + "," );
                                sw.Write( item.profileSubscriptionGroupName + "," );
                                sw.Write( item.city );
                                sw.WriteLine( "" );
                            }
                        }
                    } catch( EXISTException ex ) {
                        Console.WriteLine( ex.error.resultCode );
                        Console.WriteLine( ex.error.resultDescription );
                        Console.WriteLine( ex.error.resultType );
                    } catch( Exception ex ) {
                        Console.WriteLine( ex.Message );
                    }

                    break;

                case 8:
                    /**
                     * List Non-obligatory To Read Metering Points Service
                     **/
                    try {
                        List<ReadingMeteringPointResponse> response = epias.GetNonobligatoryToReadMeters(
                            new ListNonobligatoryToReadMetersRequest() {
                                term = new DateTime( 2018, 11, 1 ),
                                listType = "EXACT_LIST",
                                range = new Range() {
                                    begin = 0,
                                    end = 1
                                }
                            }
                        );

                        using( StreamWriter sw = new StreamWriter( "GetNonobligatoryToReadMeters_" + DateTime.Now.ToString( "yyyyMMdd_HHmm" ) + ".txt" ) ) {
                            foreach( ReadingMeteringPointResponse item in response ) {
                                sw.Write( item.meterEic + "," );
                                sw.Write( item.meterName + "," );
                                sw.Write( item.meterId + "," );
                                sw.Write( item.meterEffectiveDate + "," );
                                sw.Write( item.readingType + "," );
                                sw.Write( item.readingTypeId + "," );
                                sw.Write( item.meterLossesType + "," );
                                sw.Write( item.organization + "," );
                                sw.Write( item.meterReadingOrganization + "," );
                                sw.Write( item.profileSubscriptionGroup + "," );
                                sw.Write( item.distributionMeterCode + "," );
                                sw.Write( item.customerNo + "," );
                                sw.Write( item.meterAddress + "," );
                                sw.Write( item.countyId + "," );
                                sw.Write( item.averageAnnualConsumption + "," );
                                sw.Write( item.organizationEic + "," );
                                sw.Write( item.organizationCode + "," );
                                sw.Write( item.profileSubscriptionGroupName + "," );
                                sw.Write( item.city );
                                sw.WriteLine( "" );
                            }
                        }
                    } catch( EXISTException ex ) {
                        Console.WriteLine( ex.error.resultCode );
                        Console.WriteLine( ex.error.resultDescription );
                        Console.WriteLine( ex.error.resultType );
                    } catch( Exception ex ) {
                        Console.WriteLine( ex.Message );
                    }

                    break;

                case 9:
                    /**
                     * Service to control meter is read or not and Listing past meters
                     **/
                    try {
                        List<MeterDatas> response = epias.getMeterDataConfiguration( 
                            new DateTime( 2018, 11, 1 ) 
                        );

                        using( StreamWriter sw = new StreamWriter( "MeterDataConfiguration_" + DateTime.Now.ToString( "yyyyMMdd_HHmm" ) + ".txt" ) ) {
                            foreach( MeterDatas item in response ) {
                                sw.Write( item.meterId + "," );
                                sw.Write( item.meterEic + "," );
                                sw.Write( item.meterCity + "," );
                                sw.Write( ( item.term == null ) ? ( "," ) : ( ( (DateTime)( item.term ) ).ToString( "yyyy-MM-dd HH:mm:ss" ) + "," ) );
                                sw.Write( ( item.dataVersion == null ) ? ( "," ) : ( ( (DateTime)( item.dataVersion ) ).ToString( "yyyy-MM-dd HH:mm:ss" ) + "," ) );
                                sw.Write( ( item.confVersion == null ) ? ( "," ) : ( ( (DateTime)( item.confVersion ) ).ToString( "yyyy-MM-dd HH:mm:ss" ) + "," ) );
                                sw.Write( item.meterConsumption + "," );
                                sw.Write( item.meterGeneration + "," );
                                sw.Write( item.meterLossyConsumption + "," );
                                sw.Write( item.meterLossyGeneration + "," );
                                sw.Write( item.readingType + "," );
                                sw.Write( item.supplierOrganization + "," );
                                sw.Write( item.meterReadingCompany + "," );
                                sw.Write( item.isConfWithdrawalSettlement + "," );
                                sw.Write( item.isConfSupplySettlement + "," );
                                sw.Write( item.isConfWithdDeducSettlement + "," );
                                sw.Write( item.isConfSupplyDeducSettlement + "," );
                                sw.Write( item.isRead );
                                sw.WriteLine( "" );
                            }
                        }
                    } catch( EXISTException ex ) {
                        Console.WriteLine( ex.error.resultCode );
                        Console.WriteLine( ex.error.resultDescription );
                        Console.WriteLine( ex.error.resultType );
                    } catch( Exception ex ) {
                        Console.WriteLine( ex.Message );
                    }

                    break;
            }

            Console.WriteLine( "Finished..." );
            Console.ReadLine();
        }

        public class csettings {
            public string epias_username = string.Empty;
            public string epias_userpass = string.Empty;

            public void Get( ) {
                epias_username = Properties.Settings.Default.epias_username;
                epias_userpass = Properties.Settings.Default.epias_userpass;
            }

            public void Set( ) {
                Console.Write( "EPİAŞ / EXIST User Name : " );
                epias_username = Console.ReadLine();
                Console.Write( "EPİAŞ / EXIST Password : " );
                epias_userpass = Console.ReadLine();

                Save();
            }

            public void Show( ) {
                Console.WriteLine( "EPİAŞ / EXIST User Name : " + epias_username );
                Console.WriteLine( "EPİAŞ / EXIST Password : " + epias_userpass );
            }

            public void Save( ) {
                Properties.Settings.Default.epias_username = epias_username;
                Properties.Settings.Default.epias_userpass = epias_userpass;
                Properties.Settings.Default.Save();
            }

            public bool Check( ) {
                if( epias_username.Length == 0 || epias_userpass.Length == 0 ) {
                    return false;
                }

                return true;
            }
        }
    }
}

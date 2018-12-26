using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.IO;
using System.Net;
using System.Net.Cache;
using System.Web.Script.Serialization;

namespace EPIAS {
    class epias {
        private readonly bool test_run = false;
        public int count_perrun { get; set; } = 1000;
        public bool insane_mode { get; set; } = false;
        public string user_name { get; set; } = string.Empty;
        public string user_pass { get; set; } = string.Empty;

        private readonly string url_tgt = "cas.epias.com.tr/cas/v1/tickets?format=text";
        private readonly string url_tys = "tys.epias.com.tr";
        private readonly string url_csm = "ecms-consumption-metering-point/rest/cmp/list-changed-supplier-meters?format=json";
        private readonly string url_ddm = "ecms-consumption-metering-point/rest/cmp/list-deducted-meters?format=json";
        private readonly string url_mcr = "ecms-consumption-metering-point/rest/cmp/list-meter-count?format=json";
        private readonly string url_lme = "ecms-consumption-metering-point/rest/cmp/list-meter-eic?format=json";
        private readonly string url_mer = "ecms-consumption-metering-point/rest/cmp/list-meter-eic-range?format=json";
        private readonly string url_mpl = "ecms-consumption-metering-point/rest/cmp/listall?format=json";
        private readonly string url_mpr = "ecms-consumption-metering-point/rest/cmp/new-meters-to-be-read?format=json";
        private readonly string url_mdc = "ecms-consumption-metering-point/rest/metering/data/total/list-meter-data-configuration?format=json";

        private string tgt = string.Empty;
        private string st = string.Empty;
        private static Stopwatch swTGT = new Stopwatch();

        public epias(bool server ) {
            test_run = server;

            if( test_run) {
                url_tgt = "https://test" + url_tgt;
                url_tys = "https://test" + url_tys;
            } else {
                url_tgt = "https://" + url_tgt;
                url_tys = "https://" + url_tys;
            }
        }

        /**
         * /cmp/list-changed-supplier-meters
         * Summary : List Meters whose supplier has changed
         * Description : Returns the list of meters whose supplier has changed.
         * Parameters : GetChangedSupplierMetersRequest
         * Responses : ChangedSupplierMeterServiceResponse
        **/
        public List<ChangedSupplierMeterResponse> GetChangedSupplierMeters( DateTime term, string listType ) {
            int num_of_record = 0;

            List<ChangedSupplierMeterResponse> response = new List<ChangedSupplierMeterResponse>();

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    num_of_record = getChangedSupplierMeters( term, 0, 1, listType ).body.queryInformation.count.Value;

                    break;
                } catch( EXISTException ex) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw ex;
                    }
                }
            }

            /**
             * get all records.
             **/
            for( int i = 0; i < num_of_record; i += count_perrun ) {
                while( true ) {
                    try {
                        response = response.Concat( getChangedSupplierMeters( term, i, Math.Min( i + count_perrun - 1, num_of_record ), listType ).body.changedSupplierMeterListResponse ).ToList();

                        break;
                    } catch( Exception ex ) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw ex;
                        }
                    }
                }
            }

            return response;
        }

        private ChangedSupplierMeterServiceResponse getChangedSupplierMeters( DateTime term, int range_begin, int range_end, string listType ) {
            string request = ( new GetChangedSupplierMetersRequest() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new ListChangedSupplierMeters() {
                    term = term,
                    listType = listType,
                    range = new Range() {
                        begin = range_begin,
                        end = range_end
                    }
                }
            } ).ToString();

            string response = postRequest( request, url_tys + "/" + url_csm );
            if( response.Length != 0 ) {
                if( response.IndexOf( "SECURE" ) == -1 ) {
                    return new JavaScriptSerializer().Deserialize<ChangedSupplierMeterServiceResponse>( response );
                } else {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                }
            } else {
                return new ChangedSupplierMeterServiceResponse();
            }
        }

        /**
         * /cmp/list-deducted-meters
         * Summary : List Deducted Meters Service
         * Description : Returns the list of deducted meters in the given term.
         * Parameters : GetDeductedMetersRequest
         * Responses : DeductedMeterServiceResponse
        **/
        public List<DeductedMeterResponse> GetDeductedMeters( DateTime term ) {
            int num_of_record = 0;

            List<DeductedMeterResponse> response = new List<DeductedMeterResponse>();

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    num_of_record = GetDeductedMeters( term, 0, 1 ).body.queryInformation.count.Value;

                    break;
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw ex;
                    }
                }
            }

            /**
             * get all records.
             **/
            for( int i = 0; i < num_of_record; i += count_perrun ) {
                while( true ) {
                    try {
                        response = response.Concat( GetDeductedMeters( term, i, Math.Min( i + count_perrun - 1, num_of_record ) ).body.deductedMeterListResponse ).ToList();

                        break;
                    } catch( Exception ex ) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw ex;
                        }
                    }
                }
            }

            return response;
        }

        private DeductedMeterServiceResponse GetDeductedMeters( DateTime term, int range_begin, int range_end ) {
            string request = ( new GetDeductedMetersRequest() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new ListDeductedMetersRequest() {
                    term = term,
                    range = new Range() {
                        begin = range_begin,
                        end = range_end
                    }
                }
            } ).ToString();

            string response = postRequest( request, url_tys + "/" + url_ddm );
            if( response.Length != 0 ) {
                if( response.IndexOf( "SECURE" ) == -1 ) {
                    return new JavaScriptSerializer().Deserialize<DeductedMeterServiceResponse>( response );
                } else {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                }
            } else {
                return new DeductedMeterServiceResponse();
            }
        }

        /**
         * /cmp/list-meter-count
         * Summary : List Meter Counts
         * Description : Returns the list of meter counts with the reading type.
         * Parameters : GetMeterCountRequest
         * Responses : MeterCountServiceResponse
        **/
        public List<meterCountResponseList> GetMeterCountRequest( DateTime term, string countType ) {
            string request = ( new GetMeterCountRequest() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new ListMeterCountRequest() {
                    term = term,
                    countType = countType
                }
            } ).ToString();

            while( true ) {
                try {
                    string response = postRequest( request, url_tys + "/" + url_mcr );
                    if( response.Length != 0 ) {
                        if( response.IndexOf( "SECURE" ) == -1 ) {
                            return ( new JavaScriptSerializer().Deserialize<MeterCountServiceResponse>( response ) ).body.meterCountResponseList;
                        } else {
                            throw new EXISTException( "" ) {
                                error = new JavaScriptSerializer().Deserialize<responseError>( response )
                            };
                        }
                    } else {
                        return new List<meterCountResponseList>();
                    }
                } catch( EXISTException ex) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw ex;
                    }
                }
            }
        }

        /**
         * /cmp/list-meter-eic
         * Summary : Meter EIC Querying Service
         * Description : This service returns EIC info a meter.
         * Parameters : MeteringPointEICQueryRequest
         * Responses : MeteringPointEICQueryResponse
        **/
        public List<MeteringPointEICQueryResponseData> MeteringPointEICQueryRequest( List<MeteringPointEICQuery> meteringPointEICQueries ) {
            string request = ( new MeteringPointEICQueryRequest() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new MeteringPointEICQueryList() {
                    meteringPointEICQueries = meteringPointEICQueries
                }
            } ).ToString();

            while( true ) {
                try {
                    string response = postRequest( request, url_tys + "/" + url_lme );
                    if( response.Length != 0 ) {
                        if( response.IndexOf( "SECURE" ) == -1 ) {
                            return ( new JavaScriptSerializer().Deserialize<MeteringPointEICQueryResponse>( response ) ).body.eicQueryResponseDatas;
                        } else {
                            throw new EXISTException( "" ) {
                                error = new JavaScriptSerializer().Deserialize<responseError>( response )
                            };
                        }
                    } else {
                        return new List<MeteringPointEICQueryResponseData>();
                    }
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw ex;
                    }
                }
            }
        }

        /**
         * /cmp/list-meter-eic-range
         * Summary : List Meter Eic List Service
         * Description : Returns the list of meter eic's
         * Parameters : GetMeterEicRequest
         * Responses : MeterEicInfoServiceResponse
        **/
        public List<MeterEicInfoResponse> GetMeterEicRequest( DateTime term ) {
            string request = ( new GetMeterEicRequest() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new ListMeterEicRequest() {
                    term = term,
                    range = new Range() {
                        begin = 0,
                        end = 1
                    }
                }
            } ).ToString();

            while( true ) {
                try {
                    string response = postRequest( request, url_tys + "/" + url_mer );
                    if( response.Length != 0 ) {
                        if( response.IndexOf( "SECURE" ) == -1 ) {
                            return ( new JavaScriptSerializer().Deserialize<MeterEicInfoServiceResponse>( response ) ).body.meterEicInfoListResponse;
                        } else {
                            throw new EXISTException( "" ) {
                                error = new JavaScriptSerializer().Deserialize<responseError>( response )
                            };
                        }
                    } else {
                        return new List<MeterEicInfoResponse>();
                    }
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw ex;
                    }
                }
            }
        }

        /**
         * /cmp/listall
         * Summary : Metering Point Listing Service
         * Description : This service returns list of metering point.
         * Parameters : GetMeteringPointsRequest
         * Responses : MeteringPointServiceResponse
        **/
        public List<MeteringPointResponse> GetMeteringPointsRequest( ListMeteringPointsRequest request ) {
            int num_of_record = 0;

            List<MeteringPointResponse> response = new List<MeteringPointResponse>();

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    num_of_record = GetMeteringPointsRequest( request, 0, 1 ).body.queryInformation.count.Value;

                    break;
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw ex;
                    }
                }
            }

            /**
             * get all records.
             **/
            for( int i = 0; i < num_of_record; i += count_perrun ) {
                while( true ) {
                    try {
                        response = response.Concat( GetMeteringPointsRequest( request, i, Math.Min( i + count_perrun - 1, num_of_record ) ).body.meteringPointListResponse ).ToList();

                        break;
                    } catch( Exception ex ) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw ex;
                        }
                    }
                }
            }

            return response;
        }

        private MeteringPointServiceResponse GetMeteringPointsRequest( ListMeteringPointsRequest body, int range_begin, int range_end ) {
            string request = ( new GetMeteringPointsRequest() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new ListMeteringPointsRequest() {
                    meterEIC = body.meterEIC,
                    eligibleConsumptionType = body.eligibleConsumptionType,
                    meterEffectiveDate = body.meterEffectiveDate,
                    meterId = body.meterId,
                    meterUsageState = body.meterUsageState,
                    supplierType = body.supplierType,
                    range = new Range() {
                        begin = range_begin,
                        end = range_end
                    }
                }
            } ).ToString();

            string response = postRequest( request, url_tys + "/" + url_mpl );
            if( response.Length != 0 ) {
                if( response.IndexOf( "SECURE" ) == -1 ) {
                    return new JavaScriptSerializer().Deserialize<MeteringPointServiceResponse>( response );
                } else {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                }
            } else {
                return new MeteringPointServiceResponse();
            }
        }

        /**
         * /cmp/new-meters-to-be-read
         * Summary : List New Metering Points To Be Read Service
         * Description : If meter eic is given, it returns info of that metering point. If range is given, it returns all metering points info in that range.
         * Parameters : GetNewMeteringPointsRequest
         * Responses : ReadingMeteringPointServiceResponse
        **/
        public List<ReadingMeteringPointResponse> GetNewMeteringPointsRequest( ListNewMeteringPointsToBeRead request ) {
            int num_of_record = 0;

            List<ReadingMeteringPointResponse> response = new List<ReadingMeteringPointResponse>();

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    num_of_record = GetNewMeteringPointsRequest( request, 0, 1 ).body.queryInformation.count.Value;

                    break;
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw ex;
                    }
                }
            }

            /**
             * get all records.
             **/
            for( int i = 0; i < num_of_record; i += count_perrun ) {
                while( true ) {
                    try {
                        response = response.Concat( GetNewMeteringPointsRequest( request, i, Math.Min( i + count_perrun - 1, num_of_record ) ).body.readingMeteringPointListResponse ).ToList();

                        break;
                    } catch( Exception ex ) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw ex;
                        }
                    }
                }
            }

            return response;
        }

        private ReadingMeteringPointServiceResponse GetNewMeteringPointsRequest( ListNewMeteringPointsToBeRead body, int range_begin, int range_end ) {
            string request = ( new GetNewMeteringPointsRequest() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new ListNewMeteringPointsToBeRead() {
                    listType = body.listType,
                    meterEic = body.meterEic,
                    term = body.term,
                    range = new Range() {
                        begin = range_begin,
                        end = range_end
                    }
                }
            } ).ToString();

            string response = postRequest( request, url_tys + "/" + url_mpr );
            if( response.Length != 0 ) {
                if( response.IndexOf( "SECURE" ) == -1 ) {
                    return new JavaScriptSerializer().Deserialize<ReadingMeteringPointServiceResponse>( response );
                } else {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                }
            } else {
                return new ReadingMeteringPointServiceResponse();
            }
        }


        /**
         * /metering/data/total/list-meter-data-configuration
         * Summary : Service to control meter is read or not and Listing past meters
         * Description : With this service, users can learn if the meters are read or not and can list past meters if exists and how are the meters attached in terms of configuration. 
         * Parameters : MeteringDataConfigurationQueryRequest
         * Responses : MeteringDataAndConfigurationQueryResponse
        **/
        public List<MeterDatas> getMeterDataConfiguration( DateTime term, bool version = false ) {
            int num_of_record = 0;

            List <MeterDatas> response = new List<MeterDatas>();

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    num_of_record = getMeterDataConfiguration( term, 0, 1, version ).body.queryInformation.count.Value;

                    break;
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw ex;
                    }
                }
            }

            /**
             * get all records.
             **/
            for( int i = 0; i < num_of_record; i += count_perrun ) {
                while( true ) {
                    try {
                        response = response.Concat( getMeterDataConfiguration( term, i, Math.Min( i + count_perrun - 1, num_of_record ), version ).body.meterDatas ).ToList();

                        break;
                    } catch( Exception ex ) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw ex;
                        }
                    }
                }
            }

            return response;
        }

        private MeteringDataAndConfigurationQueryResponse getMeterDataConfiguration( DateTime term, int range_begin, int range_end, bool pastVersion ) {
            string request = ( new MeteringDataConfigurationQueryRequest() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new MeteringDataConfigurationQuery() {
                    term = term,
                    pastVersion = pastVersion,
                    meteringReadingType = "null",
                    range = new Range() {
                        begin = range_begin,
                        end = range_end
                    }
                }
            } ).ToString();

            string response = postRequest( request, url_tys + "/" + url_mdc );
            if( response.Length != 0 ) {
                if( response.IndexOf( "SECURE" ) == -1 ) {
                    return new JavaScriptSerializer().Deserialize<MeteringDataAndConfigurationQueryResponse>( response );
                } else {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                }
            } else {
                return new MeteringDataAndConfigurationQueryResponse();
            }
        }

        private string postRequest( string request, string url ) {
            if( getST() == false ) {
                return string.Empty;
            }

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create( url );

            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpWebRequest.CachePolicy = new HttpRequestCachePolicy( HttpRequestCacheLevel.NoCacheNoStore );
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = request.Length;
            httpWebRequest.Host = (new Uri( url_tys )).Host;
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add( "Charset", "UTF-8" );
            httpWebRequest.Headers.Add( "ecms-service-ticket", st );

            using( StreamWriter streamWriter = new StreamWriter( httpWebRequest.GetRequestStream() ) ) {
                streamWriter.Write( request );
                streamWriter.Flush();
                streamWriter.Close();
            }

            try {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using( StreamReader streamReader = new StreamReader( httpResponse.GetResponseStream() ) ) {
                    return streamReader.ReadToEnd();
                }
            } catch( Exception e ) {
                throw new Exception( e.Message );
            }
        }

        private bool getST() {
            if( getTGT() == false ) {
                return false;
            }

            string request = "service=" + url_tys;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create( tgt + "?format=text" );

            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpWebRequest.CachePolicy = new HttpRequestCachePolicy( HttpRequestCacheLevel.NoCacheNoStore );
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.ContentLength = request.Length;
            httpWebRequest.Host = ( new Uri( tgt ) ).Host;
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add( "Charset", "UTF-8" );

            using( StreamWriter streamWriter = new StreamWriter( httpWebRequest.GetRequestStream() ) ) {
                streamWriter.Write( request );
                streamWriter.Flush();
                streamWriter.Close();
            }

            try {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using( StreamReader streamReader = new StreamReader( httpResponse.GetResponseStream() ) ) {
                    st = streamReader.ReadToEnd();

                    return true;
                }
            } catch {
                throw new Exception( "Error on getting ST" );
            }
        }

        private bool getTGT() {
            if( user_name.Length == 0 || user_pass.Length == 0 ) {
                throw new Exception( "EXIST username and / or password is empty." );
            }

            if( swTGT.IsRunning == true && swTGT.Elapsed.TotalMinutes < 44 ) {
                return true;
            }

            string request = "username=" + user_name + "&password=" + user_pass;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create( url_tgt );

            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpWebRequest.CachePolicy = new HttpRequestCachePolicy( HttpRequestCacheLevel.NoCacheNoStore );
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.ContentLength = request.Length;
            httpWebRequest.Host = (new Uri( url_tgt ) ).Host;
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Method = "POST";

            httpWebRequest.Headers.Add( "Charset", "UTF-8" );

            if( swTGT.IsRunning ) {
                swTGT.Reset();
            } else {
                swTGT.Start();
            }

            using( StreamWriter streamWriter = new StreamWriter( httpWebRequest.GetRequestStream() ) ) {
                streamWriter.Write( request );
                streamWriter.Flush();
                streamWriter.Close();
            }

            try {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using( StreamReader streamReader = new StreamReader( httpResponse.GetResponseStream() ) ) {
                    tgt = httpResponse.GetResponseHeader( "Location" );

                    return true;
                }
            } catch(Exception ex) {
                if( ex.Message.IndexOf( "Bad Request" ) != -1 || ex.Message.IndexOf( "Hatalı İstek" ) != -1 ) {
                    throw new EXISTException() {
                        error = new responseError() {
                            resultCode = "BADREQUEST",
                            resultDescription = "Username or password are wrong.",
                            resultType = "SECURITYERROR"
                        }
                    };
                } else {
                    throw new Exception( "Error on getting TGT" );
                }
            }
        }
    }

    /**
     * list-changed-supplier-meters
     **/
    /**
     * request
     **/
    /**
	 * GetChangedSupplierMetersRequest
	 * Wrapper Request Model for Changed-Supplier Meters
	**/
    public class GetChangedSupplierMetersRequest {
        public List<Header> header { get; set; } // Keeps request header informations.
        public ListChangedSupplierMeters body { get; set; } // Changed-Supplier Meters Request Model

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /**
	 * ListChangedSupplierMeters
	 * Changed-Supplier Meters Request Model
	**/
    public class ListChangedSupplierMeters {
        public Range range { get; set; } // By using this object you can specify record number with start and end index values.
        public DateTime term { get; set; } // Meter Term
        public string listType { get; set; } //ENUM: PRE_LIST, EXACT_LIST -- Ön liste veya kesin liste durumunu belirtir. PRE_LIST:Ön Liste , EXACT_LIST = Kesin Liste

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"listType\":\"" + listType + "\",\"range\":" + range.ToString() + "}";
        }
    }

    /**
     * response
     **/
    /**
	 * ChangedSupplierMeterServiceResponse
	 * It keeps the response info of meters whose supplier is changed.
	**/
    public class ChangedSupplierMeterServiceResponse {
        public string resultCode { get; set; } // 0 means success other values may differ for each request
        public string resultDescription { get; set; } // if requests succeed return OK otherwise returns error description
        public string resultType { get; set; } //ENUM: SUCCESS, BUSINESSERROR, SYSTEMERROR, SECURITYERROR -- returns SUCCESS for valid operation, if you violate a business rule you will get BUSINESSERROR , if our system can not process your request, you will get SYSTEMERROR
        public ChangedSupplierMeterListResponse body { get; set; } // It keeps the response body info of meters whose supplier is changed.
    }

    /**
	 * ChangedSupplierMeterListResponse
	**/
    public class ChangedSupplierMeterListResponse {
        public QueryInformation queryInformation { get; set; } // Keeps how many record exist in the service response and range values.
        public List<ChangedSupplierMeterResponse> changedSupplierMeterListResponse { get; set; }
    }

    /**
     * ChangedSupplierMeterResponse
    **/
    public class ChangedSupplierMeterResponse {
        public int? newMeterId { get; set; } // New Meter's Id
        public string newMeterEic { get; set; } // New Meter's EIC
        public string newOrganizationEic { get; set; } // New Meter's Organization EIC
        public string oldOrganizationEic { get; set; } // Old Meter's Organization EIC
        public string newCustomerNo { get; set; } // New Customer No
        public string newMeterName { get; set; } // New Meter's Name
        public string newMeterAddress { get; set; } // New Meter's Address
        public int? newMeterCountyId { get; set; } // New Meter's Country Id
        public int? newMeterReadingType { get; set; } // New Meter's Reading Type
        public string newMeterReadingTypeEnum { get; set; } //ENUM: THREE_RATE, HOURLY, SINGLE_RATE -- New Meter' s Reading Type as Enum
        public int? newProfileSubscriptionGroup { get; set; } // New Meter's Subscription Group
        public double? newAverageAnnualConsumption { get; set; } // New Meter's Average Annual Consumption
        public string newDistributionMeterCode { get; set; } // New Meter's Distribution Meter Code
        public string newprofileSubscriptionGroupName { get; set; } // New Meter's Subscription Group Name
        public string newCity { get; set; } // New Meter's City
        public string oldOrganizationCode { get; set; } // Old Meter's Organization Code
        public string newOrganizationCode { get; set; } // New Meter's Organization Code
    }

    /**
     * list-deducted-meters
     **/
    /**
     * request
     **/
    /**
	 * GetDeductedMetersRequest
	 * Wrapper Request Model for Deducted Meters
	**/
    public class GetDeductedMetersRequest {
        public List<Header> header { get; set; } // Keeps request header informations.
        public ListDeductedMetersRequest body { get; set; } // Deducted Meters Request Model

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /**
	 * ListDeductedMetersRequest
	 * Deducted Meters Request Model
	**/
    public class ListDeductedMetersRequest {
        public Range range { get; set; } // By using this object you can specify record number with start and end index values.
        public DateTime term { get; set; } // Meter Term

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"range\":" + range.ToString() + "}";
        }
    }

    /**
     * response
     **/
    /**
	 * DeductedMeterServiceResponse
	 * It keeps the response info of deducted meters.
	**/
    public class DeductedMeterServiceResponse {
        public string resultCode { get; set; } // 0 means success other values may differ for each request
        public string resultDescription { get; set; } // if requests succeed return OK otherwise returns error description
        public string resultType { get; set; } //ENUM: SUCCESS, BUSINESSERROR, SYSTEMERROR, SECURITYERROR -- returns SUCCESS for valid operation, if you violate a business rule you will get BUSINESSERROR , if our system can not process your request, you will get SYSTEMERROR
        public DeductedMeterListResponse body { get; set; } // It keeps the response body info of deducted meters.
    }

    /**
	 * DeductedMeterListResponse
	**/
    public class DeductedMeterListResponse {
        public QueryInformation queryInformation { get; set; } // Keeps how many record exist in the service response and range values.
        public List<DeductedMeterResponse> deductedMeterListResponse { get; set; }
    }

    /**
	 * DeductedMeterResponse
	**/
    public class DeductedMeterResponse {
        public int? meterId { get; set; } // Meter Id
        public DateTime? meterEffectiveDate { get; set; } // Meter Effective Date
        public string meterEic { get; set; } // Meter EIC
        public string city { get; set; } // City
        public string meterSerialNo { get; set; } // Meter Serial No
        public string customerNo { get; set; } // Customer No
        public string meterName { get; set; } // Meter Name
        public int? settlementPointId { get; set; } // Settlement Point Id
        public string settlementPointName { get; set; } // Settlement Point Name
    }

    /**
     * list-meter-count
     **/
    /*
     * request
     **/
    /**
	 * ListMeterCountRequest
	 * Meter Count Request Model
	**/
    public class ListMeterCountRequest {
        public DateTime term { get; set; } // Meter Term
        public string countType { get; set; } //ENUM: RELATED, PORTFOLIO -- Meter Count Type Request Model

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"countType\":\"" + countType + "\"}";
        }
    }

    /**
	 * GetMeterCountRequest
	 * Wrapper Request Model for Meter Count
	**/
    public class GetMeterCountRequest {
        public List<Header> header { get; set; } // Keeps request header informations.
        public ListMeterCountRequest body { get; set; } // Meter Count Request Model

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /*
     * response
     **/
    /**
	 * MeterCountServiceResponse
	 * Service Response for Listing Meter Counts
	**/
    public class MeterCountServiceResponse {
        public string resultCode { get; set; } // 0 means success other values may differ for each request
        public string resultDescription { get; set; } // if requests succeed return OK otherwise returns error description
        public string resultType { get; set; } //ENUM: SUCCESS, BUSINESSERROR, SYSTEMERROR, SECURITYERROR -- returns SUCCESS for valid operation, if you violate a business rule you will get BUSINESSERROR , if our system can not process your request, you will get SYSTEMERROR
        public ResponseBody body { get; set; } // Service Response Body for Listing Meter Counts
    }

    /**
	 * ResponseBody
	**/
    public class ResponseBody {
        public List<meterCountResponseList> meterCountResponseList { get; set; }
    }

    /**
     * meterCountResponseList
     **/
    public class meterCountResponseList {
        public double meterEffectiveDate { get; set; }
        public string readingType { get; set; }
        public int meterCount { get; set; }
    }

    /**
     * list-meter-eic
     **/
    /*
     * request
     **/
    /**
	 * MeteringPointEICQueryRequest
	 * Request Model of Meter Eic Querying which has header and body info.
	**/
    public class MeteringPointEICQueryRequest {
        public List<Header> header { get; set; } // Keeps request header informations.
        public MeteringPointEICQueryList body { get; set; } // Service Request Body.

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /**
	 * MeteringPointEICQueryList
	 * Request Container for Metering Point EIC Querying Service.
	**/
    public class MeteringPointEICQueryList {
        public List<MeteringPointEICQuery> meteringPointEICQueries { get; set; } // Request Model List for Metering Point EIC Querying Service.

        public override string ToString( ) {
            return "{\"meteringPointEICQueries\":[" + string.Join<MeteringPointEICQuery>( ",", meteringPointEICQueries.ToArray() ) + "]}";
        }
    }

    /**
	 * MeteringPointEICQuery
	 * Request Model for Metering Point EIC Querying Service.
	**/
    public class MeteringPointEICQuery {
        public string meterEic { get; set; } // It keeps EIC of requested meter.
        public string distributionMeterCode { get; set; } // It keeps distribution meter code.
        public string meterReadingCompanyEic { get; set; } // It keeps EIC of the company who reads requested meter.

        public override string ToString( ) {
            return "{\"meterEic\":\"" + meterEic + "\",\"distributionMeterCode\":\"" + distributionMeterCode + "\",\"meterReadingCompanyEic\":\"" + meterReadingCompanyEic + "\"}";
        }
    }

    /*
     * response
     **/
    /**
	 * MeteringPointEICQueryResponse
	 * Response Model for Meter EIC Querying
	**/
    public class MeteringPointEICQueryResponse {
        public string resultCode { get; set; } // 0 means success other values may differ for each request
        public string resultDescription { get; set; } // if requests succeed return OK otherwise returns error description
        public string resultType { get; set; } //ENUM: SUCCESS, BUSINESSERROR, SYSTEMERROR, SECURITYERROR -- returns SUCCESS for valid operation, if you violate a business rule you will get BUSINESSERROR , if our system can not process your request, you will get SYSTEMERROR
        public MeteringPointEICQueryResponseDataList body { get; set; } // Response Body for Metering Point EIC Querying Service
    }

    /**
	 * MeteringPointEICQueryResponseDataList
	 * Response container model for Metering Point EIC Querying Service
	**/
    public class MeteringPointEICQueryResponseDataList {
        public List<MeteringPointEICQueryResponseData> eicQueryResponseDatas { get; set; } // Response List for Metering Point EIC Querying Service
    }

    /**
	 * MeteringPointEICQueryResponseData
	 * Response Model for Metering Point EIC Querying Service.
	**/
    public class MeteringPointEICQueryResponseData {
        public string meterEic { get; set; } // It keeps info of Meter EIC.
        public string distributionMeterId { get; set; } // It keeps Distribution Meter Code which belongs to Meter's Company.
        public string customerNo { get; set; } // It keeps Customer No of the meter.
        public string eligibleConsumptionType { get; set; } //ENUM: ELIGIBLE_CONSUMER, NO_ELIGIBLE_CONSUMER, ORGANIZED_INDUSTRIAL_ZONE_EC -- Consumption Metering Type
        public string meterUsageState { get; set; } //ENUM: NO_CONSUMER, IN_USE -- Usage Type
        public string supplierType { get; set; } //ENUM: END_USE_SUPPLIER, BILATERAL_CONTRACT, RETAIL_SALE -- Supply Type
        public string meteringPointName { get; set; } // It keeps Meter Name.
        public string meteringAddress { get; set; } // It keeps info of Meter Address.
        public int? cityId { get; set; } // It keeps City Id which the meter is belonging to.
        public int? countyId { get; set; } // It keeps County Id which the meter is belonging to.
        public string meterReadingCompanyId { get; set; } // It keeps Meter Reading Company Id.
        public string meterReadingCompanyEic { get; set; } // Sayacı okuyan kurumun EIC bilgisini tutar
        public string status { get; set; } //ENUM: SUCCESS, NOT_FOUND -- Yapılan sorgulama sonucu kayıt bulunma durum bilgisini tutar. SUCCES:Ölçüm Noktası Bulundu , NOT_FOUND: Ölçüm Noktası Bulunamadı
        public string description { get; set; } // Yapılan sorgulama sonucu başarısız olması durumunda nedeni.
    }

    /**
     * list-meter-eic-range
     **/
    /*
     * request
     **/
    /**
	 * GetMeterEicRequest
	 * Wrapper Request Model for Listing Meter EIC With Range
	**/
    public class GetMeterEicRequest {
        public List<Header> header { get; set; } // Keeps request header informations.
        public ListMeterEicRequest body { get; set; } // Meter EIC Request Model

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /**
	 * ListMeterEicRequest
	 * Meter EIC Request Model
	**/
    public class ListMeterEicRequest {
        public Range range { get; set; } // By using this object you can specify record number with start and end index values.
        public DateTime term { get; set; } // Meter Term

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"range\":" + range.ToString() + "}";
        }
    }

    /*
     * response
     **/
    /**
	 * MeterEicInfoServiceResponse
	 * It keeps the response info of meter eic.
	**/
    public class MeterEicInfoServiceResponse {
        public string resultCode { get; set; } // 0 means success other values may differ for each request
        public string resultDescription { get; set; } // if requests succeed return OK otherwise returns error description
        public string resultType { get; set; } //ENUM: SUCCESS, BUSINESSERROR, SYSTEMERROR, SECURITYERROR -- returns SUCCESS for valid operation, if you violate a business rule you will get BUSINESSERROR , if our system can not process your request, you will get SYSTEMERROR
        public MeterEicInfoListResponse body { get; set; } // It keeps the response body info of meter eic.
    }

    /**
	 * MeterEicInfoListResponse
	**/
    public class MeterEicInfoListResponse {
        public QueryInformation queryInformation { get; set; } // Keeps how many record exist in the service response and range values.
        public List<MeterEicInfoResponse> meterEicInfoListResponse { get; set; }
    }

    /**
	 * MeterEicInfoResponse
	**/
    public class MeterEicInfoResponse {
        public string meterEic { get; set; } // Meter EIC
        public string readingType { get; set; } //ENUM: THREE_RATE, HOURLY, SINGLE_RATE -- Meter Reading Type
        public int? meterId { get; set; } // Meter Id
    }

    /**
     * listall
     **/
    /*
     * request
     **/
    /**
	 * GetMeteringPointsRequest
	 * Wrapper Request Model for Listing Meters
	**/
    public class GetMeteringPointsRequest {
        public List<Header> header { get; set; } // Keeps request header informations.
        public ListMeteringPointsRequest body { get; set; } // Metering Point Request Model (Fill either meter id or meter eic)

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /**
	 * ListMeteringPointsRequest
	 * Metering Point Request Model (Fill either meter id or meter eic)
	**/
    public class ListMeteringPointsRequest {
        public Range range { get; set; } // By using this object you can specify record number with start and end index values.
        public int? meterId { get; set; } // Meter Id.
        public string meterEIC { get; set; } // Meter EIC.
        public string meterDistributionCode { get; set; } // Distribution Meter Code.
        public string eligibleConsumptionType { get; set; } //ENUM: ELIGIBLE_CONSUMER, NO_ELIGIBLE_CONSUMER, ORGANIZED_INDUSTRIAL_ZONE_EC -- Eligible Consumption Type
        public string meterUsageState { get; set; } //ENUM: NO_CONSUMER, IN_USE -- Meter usage state
        public string supplierType { get; set; } //ENUM: END_USE_SUPPLIER, BILATERAL_CONTRACT, RETAIL_SALE, NONE -- Ölçüm noktası tedarik tipi.
        public DateTime? meterEffectiveDate { get; set; } // Eligible Consumption Type effective period

        public override string ToString( ) {
            return "{\"meterEIC\":\"" + meterEIC + "\",\"eligibleConsumptionType\":\"" + eligibleConsumptionType + "\",\"meterEffectiveDate\":\"" + proGEDIA.ToString( meterEffectiveDate.Value ) + "\",\"meterId\":\"" + meterId + "\",\"meterUsageState\":\"" + meterUsageState + "\",\"supplierType\":\"" + supplierType + "\",\"range\":" + range.ToString() + "}";
        }
    }

    /*
     * response
     **/
    /**
	 * MeteringPointServiceResponse
	 * It keeps the response info of metering point.
	**/
    public class MeteringPointServiceResponse {
        public string resultCode { get; set; } // 0 means success other values may differ for each request
        public string resultDescription { get; set; } // if requests succeed return OK otherwise returns error description
        public string resultType { get; set; } //ENUM: SUCCESS, BUSINESSERROR, SYSTEMERROR, SECURITYERROR -- returns SUCCESS for valid operation, if you violate a business rule you will get BUSINESSERROR , if our system can not process your request, you will get SYSTEMERROR
        public MeteringPointListResponse body { get; set; } // It keeps the response body info of metering point.
    }

    /**
	 * MeteringPointListResponse
	**/
    public class MeteringPointListResponse {
        public QueryInformation queryInformation { get; set; } // Keeps how many record exist in the service response and range values.
        public List<MeteringPointResponse> meteringPointListResponse { get; set; }
    }

    /**
	 * MeteringPointResponse
	**/
    public class MeteringPointResponse {
        public int? id { get; set; } // Meter Id
        public string meterEic { get; set; } // Meter EIC
        public string distributionMeterId { get; set; } // Distribution Meter Code
        public bool? status { get; set; } // Meter Status
        public int? meterReadingCompany { get; set; } // Meter Reading Organization
        public string withDrawalDeducSettlementPointEic { get; set; } // Withdrawal Deduction Settlement Point
        public string meterName { get; set; } // Meter Name
        public string meterAddress { get; set; } // Meter Address
        public int? countyId { get; set; } // County Id
        public double? transformerInputVoltage { get; set; } // Transformer Input Voltage
        public double? transformerOutputVoltage { get; set; } // Transformer Output Voltage
        public string customerNo { get; set; } // Customer No
        public int? substation { get; set; } // Transformer That Meter Belongs To
        public int? connectionPointLocation { get; set; } // Connection Point Location
        public string connectionPointLocationOther { get; set; } // Other Connection Point Location
        public int? meteringVoltage { get; set; } // Meter Connection Point Voltage
        public int? profileType { get; set; } // Profile Type (AC / DC)
        public int? profileSubscriptionGroup { get; set; } // Meter Subscription Group
        public int? readingType { get; set; } // Meter Reading Type
        public double? transformerPower { get; set; } // Transformer Power
        public int? busbarVoltage { get; set; } // Busbar Voltage
        public double? noLoadLoss { get; set; } // Loss On No-load
        public double? loadLoss { get; set; } // Loss On Load
        public int? temperatureCoefficient { get; set; } // Conductor Resistance
        public double? lineLength { get; set; } // Line Length
        public double? lineSection { get; set; } // Line Section
        public int? lineCircuit { get; set; } // Number of Circuits in Line
        public bool? transLossFactorStatus { get; set; } // Transfer Loss Factor Status
        public double? averageAnnualConsumption { get; set; } // Average Annual Consumption
        public int? supplyPosition { get; set; } // Meter Supply Position
        public int? withdrawalPosition { get; set; } // Meter Withdrawal Position
        public Int64? addressCode { get; set; } // Adres kod bilgisini tutar.
        public bool? zonningPosition { get; set; } // ?mar yerle?im alan? konum bilgisini tutar.
        public int? usageState { get; set; } // Ölçüm noktas? kullan?m durumu bilgisini tutar.
        public string organizedIndustrialZoneEic { get; set; } // lçüm noktas?n?n hangi organize sanayi bölgesinin ana sayac? oldu?u bilgisini tutar.
        public bool? canLoadProfile { get; set; } // Yük profil bilgisini al?nabilir bilgisini tutar.
        public string mainEligibleConsumptionEic { get; set; } // OSB'lerin alt?ndaki ölçüm noktalar?n?n hangi ana sayaca ba?l? oldu?u bilgisini tutar.
        public string supplyDeductionSettlementPoint { get; set; } // OSB'lerin ana sayac?n?n veri? tenzil uevçb bilgisini tutar.
        public int? eligibleConsumptionType { get; set; } // Ölçüm noktas?n?n tip bilgisini tutar.
        public bool? amr { get; set; } // Otomatik sayaç okuma sistemi ile okunmad? durum bilgisini tutar.
        public double? maxAnnualConsumption { get; set; } // Ölçüm noktas?n?n y?ll?k maksimum tüketim de?eri bilgisini tutar.
        public bool? estimation { get; set; } // Ölçüm noktas?n?n tahminleme yap?lma durum bilgisi.
        public string serialNumber { get; set; } // Serial number
        public string manufacturer { get; set; } // Manufacturer
        public string supplierOrganization { get; set; } // Supplier Organizaton
        public double? contractPower { get; set; } // Contract power
        public string tariffClass { get; set; } //ENUM: SINGLE_TERM, DOUBLE_TERM -- Tariff class
        public string mainTariffGroup { get; set; } //ENUM: INDUSTRY, BUSINESS, RESIDENCE, AGRICULTURAL_IRRIGATION, LIGHTING -- Main tariff group
        public string activityCode { get; set; } // Activity code
        public int? meteringType { get; set; } // Metering Type
        public int? supplierType { get; set; } // Supplier Type
        public string withDrawalSettlementPointEic { get; set; } // Withdrawal Deduction Settlement Point
        public string supplySettlementPointEic { get; set; } // Withdrawal Deduction Settlement Point
        public string withdrawalPositionDescription { get; set; } // Ba?lant? pozisyon aç?klama bilgisi
        public string countyName { get; set; } // Meter's County Name
        public string organizedIndustrialZone { get; set; } // Organize Sanayi Bölgesinin Adı
        public double? meteringVoltageValue { get; set; } // Bağlantı noktası Gerilim Değeri
        public double? busbarVoltageValue { get; set; } // Bara Gerilim Değeri
        public string lineCircuitDesc { get; set; } // Hat Devre Bilgisi
        public double? temperatureCoefficientValue { get; set; } // İletken Özdirenç Değeri
        public string profileSubscriptionGroupDesc { get; set; } // Profil Abone Grup Bilgisi
        public string profileTypeDesc { get; set; } // Profil Tip Bilgisi
        public string withdrawalPositionName { get; set; } // Sayaç Çekiş Pozisyon Adı
        public string supplyPositionName { get; set; } // Sayaç Veriş Pozisyon Adı
        public string eligibleConsumptionTypeDesc { get; set; } //ENUM: ELIGIBLE_CONSUMER, NO_ELIGIBLE_CONSUMER, ORGANIZED_INDUSTRIAL_ZONE_EC -- Ölçüm Noktası Tip Bilgisi
        public string usageStateDesc { get; set; } //ENUM: NO_CONSUMER, IN_USE -- Ölçüm Noktası Kullanım Durum Bilgisi
        public string supplierTypeDesc { get; set; } //ENUM: END_USE_SUPPLIER, BILATERAL_CONTRACT, RETAIL_SALE -- Tedarik Tip Bilgisi
        public string connectionPointLocationDesc { get; set; } // Trafoya Gören Konum Bilgisi
        public string substationDesc { get; set; } // Trafo Merkezi Bilgisi
        public DateTime? registrationDate { get; set; } // Ölçüm noktası kayıt tarih bilgisi.
    }

    /**
      * GetNewMeteringPointsRequest
      **/
    /*
     * request
     **/
    /**
     * GetNewMeteringPointsRequest
     * Wrapper Request Model for New Metering Points To Be Read
     **/
    public class GetNewMeteringPointsRequest {
        public List<Header> header { get; set; } // Keeps request header informations.
        public ListNewMeteringPointsToBeRead body { get; set; } // New Metering Points To Be Read Request Model

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /**
     * ListNewMeteringPointsToBeRead
     * New Metering Points To Be Read Request Model
     **/
    public class ListNewMeteringPointsToBeRead {
        public Range range { get; set; } // By using this object you can specify record number with start and end index values.
        public string listType { get; set; } //ENUM: PRE_LIST, EXACT_LIST -- Ön liste veya kesin liste durumunu belirtir. PRE_LIST:Ön Liste , EXACT_LIST = Kesin Liste
        public DateTime term { get; set; } // Meter Term
        public string meterEic { get; set; } // Meter EIC

        public override string ToString( ) {
            return "{\"range\":" + range.ToString() + ",\"listType\":\"" + listType + "\",\"term\":\"" + proGEDIA.ToString( term ) + "\",\"meterEic\":\"" + meterEic + "\"}";
        }
    }

    /*
     * response
     **/
    /**
	 * ReadingMeteringPointServiceResponse
	 * It keeps the service response of meters based on their reading type (new to be read or non-obligatory to read).
	**/
    public class ReadingMeteringPointServiceResponse {
        public string resultCode { get; set; } // 0 means success other values may differ for each request
        public string resultDescription { get; set; } // if requests succeed return OK otherwise returns error description
        public string resultType { get; set; } //ENUM: SUCCESS, BUSINESSERROR, SYSTEMERROR, SECURITYERROR -- returns SUCCESS for valid operation, if you violate a business rule you will get BUSINESSERROR , if our system can not process your request, you will get SYSTEMERROR
        public ReadingMeteringPointListResponse body { get; set; } // It keeps the response body info of meters based on their reading type (new to be read or non-obligatory to read).
    }

    /**
	 * ReadingMeteringPointListResponse
	 * It keeps the response body info of meters based on their reading type (new to be read or non-obligatory to read).
	**/
    public class ReadingMeteringPointListResponse {
        public QueryInformation queryInformation { get; set; } // Keeps how many record exist in the service response and range values.
        public List<ReadingMeteringPointResponse> readingMeteringPointListResponse { get; set; } // It keeps the response info of meters based on their reading type (new to be read or non-obligatory to read).
    }

    /**
	 * ReadingMeteringPointResponse
	 * It keeps the response info of meters based on their reading type (new to be read or non-obligatory to read).
	**/
    public class ReadingMeteringPointResponse {
        public string meterEic { get; set; } // Meter EIC
        public string meterName { get; set; } // Meter Name
        public int? meterId { get; set; } // Meter Id
        public DateTime? meterEffectiveDate { get; set; } // Meter Effective Date
        public string readingType { get; set; } //ENUM: THREE_RATE, HOURLY, SINGLE_RATE -- Meter Reading Type
        public int? readingTypeId { get; set; } // Meter Reading Type
        public int? meterLossesType { get; set; } // Meter Losses Type
        public int? organization { get; set; } // Organization Id
        public int? meterReadingOrganization { get; set; } // Meter Reading Organization
        public int? profileSubscriptionGroup { get; set; } // Meter Subscription Group
        public string distributionMeterCode { get; set; } // Distribution Meter Code
        public string customerNo { get; set; } // Customer No
        public string meterAddress { get; set; } // Meter Address
        public int? countyId { get; set; } // County Id
        public double? averageAnnualConsumption { get; set; } // Average Annual Consumption
        public string organizationEic { get; set; } // Organization EIC
        public string organizationCode { get; set; } // Organization Code
        public string profileSubscriptionGroupName { get; set; } // Meter Subscription Group Name
        public string city { get; set; } // City that meter belongs to.
    }

    /**
     * list-meter-data-configuration
     **/
    /**
     * request
     **/
    /**
	 * MeteringDataConfigurationQueryRequest
	 * Request for listing meter's withdrawal/supply and past meters 
	**/
    public class MeteringDataConfigurationQueryRequest {
        public List<Header> header { get; set; } // Keeps request header informations.
        public MeteringDataConfigurationQuery body { get; set; } // Keeps request body informations.

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /**
	 * MeteringDataConfigurationQuery
	 * Request for Total Meter Withdrawal / Supply Service
	**/
    public class MeteringDataConfigurationQuery {
        public Range range { get; set; } // By using this object you can specify record number with start and end index values.
        public DateTime term { get; set; } // Meter term
        public string meteringReadingType { get; set; } //ENUM: THREE_RATE, HOURLY, SINGLE_RATE -- Meter reading type (if all meters wanted, this field should be null)
        public bool? pastVersion { get; set; } // If past meters are going to be listed or not

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"pastVersion\":" + meteringReadingType + ",\"range\":" + range.ToString() + "}";
        }
    }

    /**
     * response
     **/
    /**
	 * MeteringDataAndConfigurationQueryResponse
	 * It keeps withdrawal/supply values and Past Meter info.
	**/
    public class MeteringDataAndConfigurationQueryResponse {
        public string resultCode { get; set; } // 0 means success other values may differ for each request
        public string resultDescription { get; set; } // if requests succeed return OK otherwise returns error description
        public string resultType { get; set; } //ENUM: SUCCESS, BUSINESSERROR, SYSTEMERROR, SECURITYERROR -- returns SUCCESS for valid operation, if you violate a business rule you will get BUSINESSERROR , if our system can not process your request, you will get SYSTEMERROR
        public MeteringDataAndConfigurationList body { get; set; } // It keeps withdrawal/supply values and Past Meter info.
    }

    /**
	 * MeteringDataAndConfigurationList
	**/
    public class MeteringDataAndConfigurationList {
        public QueryInformation queryInformation { get; set; } // Keeps how many record exist in the service response and range values.
        public List<MeterDatas> meterDatas { get; set; } // It keeps withdrawal/supply values and Past Meter info.
    }

    /**
	 * MeterDatas
	 * Meter data configuration response
	**/
    public class MeterDatas {
        public int meterId { get; set; } // Meter Id
        public string meterEic { get; set; } // Meter EIC
        public string meterName { get; set; } // Meter name
        public string meterCity { get; set; } // Meter's city
        public DateTime term { get; set; } // Meter term
        public DateTime dataVersion { get; set; } // Meter data version
        public DateTime confVersion { get; set; } // Meter configuration version
        public double meterConsumption { get; set; } // Meter's consumption value
        public double meterGeneration { get; set; } // Meter's generation value
        public double meterLossyConsumption { get; set; } // Meter's lossy consumption value
        public double meterLossyGeneration { get; set; } // Meter's lossy generation value
        public string readingType { get; set; } //ENUM: THREE_RATE, HOURLY, SINGLE_RATE -- Meter reading type
        public string supplierOrganization { get; set; } // Meter supplier
        public string meterReadingCompany { get; set; } // Meter reading company
        public bool isConfWithdrawalSettlement { get; set; } // Info of If meter is attached to the related supplier's withdrawal settlement point
        public bool isConfSupplySettlement { get; set; } // Info of If meter is attached to the related supplier's supply settlement point
        public bool isConfWithdDeducSettlement { get; set; } // Info of If meter is attached to the related supplier's deducted withdrawal settlement point
        public bool isConfSupplyDeducSettlement { get; set; } // Info of If meter is attached to the related supplier's deducted supply settlement point
        public bool isRead { get; set; } // Meter is read or not
        public string meterReadingCompanyEic { get; set; } // Meter reading company EIC
        public string supplierOrganizationEic { get; set; } // Meter supplier company EIC
    }

    /**
     * Common classes
     **/
    /**
	 * QueryInformation
	 * Keeps how many record exist in the service response and range values.
	**/
    public class QueryInformation {
        public int? begin { get; set; } // Start range value.
        public int? end { get; set; } // End range value.
        public int? count { get; set; } // Total number of record.
    }

    /**
	 * Header
	 * Keeps request header informations.
	**/
    public class Header {
        public string key { get; set; } // Keeps header key information.
        public string value { get; set; } // Keeps header value information.

        public Header( string key, string value ) {
            this.key = key;
            this.value = value;
        }

        public override string ToString( ) {
            return "{\"key\":\"" + key + "\",\"value\":\"" + value + "\"}";
        }
    }

    /**
	 * Range
	 * By using this object you can specify record number with start and end index values.
	**/
    public class Range {
        public int? begin { get; set; } // Keeps range start value.
        public int? end { get; set; } // Keeps range end value.

        public override string ToString( ) {
            return "{\"begin\": " + begin + ",\"end\": " + end + "}";
        }
    }

    public class responseError {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public string resultType { get; set; }
    }

    public class EXISTException : Exception {
        public responseError error = new responseError();

        public EXISTException( ) : base() {

        }

        public EXISTException( string message ) : base( message ) {

        }
    }

    public static class proGEDIA {
        public static string ToString( DateTime dt ) {
            if( dt >= new DateTime( 2016, 10, 1, 0, 0, 0 ) || ( dt.Year == 2015 && dt.Month == 11 ) ) {
                return dt.ToString( "yyyy-MM-dd\\T00:00:00.000" ) + "+0300";
            } else if( dt.Month > 10 || dt.Month < 4 ) {
                return dt.ToString( "yyyy-MM-dd\\T00:00:00.000" ) + "+0200";
            } else {
                return dt.ToString( "yyyy-MM-dd\\T00:00:00.000" ) + "+0300";
            }
        }
    }
}

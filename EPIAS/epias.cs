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
        public int count_perrun { get; set; } = 1000;
        public bool insane_mode { get; set; } = false;
        public string user_name { get; set; } = string.Empty;
        public string user_pass { get; set; } = string.Empty;

        private readonly string url_tgt = "https://testcas.epias.com.tr/cas/v1/tickets?format=text";
        private readonly string url_tys = "https://testtys.epias.com.tr";
        private readonly string url_mdc = "ecms-consumption-metering-point/rest/metering/data/total/list-meter-data-configuration?format=json";
        private readonly string url_csm = "ecms-consumption-metering-point/rest/cmp/list-changed-supplier-meters?format=json";
        private readonly string url_ddm = "ecms-consumption-metering-point/rest/cmp/list-deducted-meters?format=json";
        private readonly string url_mcr = "ecms-consumption-metering-point/rest/cmp/list-meter-count?format=json";
        private readonly string url_lme = "ecms-consumption-metering-point/rest/cmp/list-meter-eic?format=json";

        private string tgt = string.Empty;
        private string st = string.Empty;
        private static Stopwatch swTGT = new Stopwatch();

        /**
         * List Meters whose supplier has changed
         **/
        public List<ChangedSupplierMeterResponse> getChangedSupplierMeters( DateTime term, string listType ) {
            List<ChangedSupplierMeterResponse> response = new List<ChangedSupplierMeterResponse>();
            ChangedSupplierMeterServiceResponse partial;

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    partial = getChangedSupplierMeters( term, 0, 1, listType );

                    break;
                } catch( EXISTException ex) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw new Exception( "error on getting meter data configuration" );
                    }
                }
            }

            /**
             * get all records.
             **/
            int num_of_record = partial.body.queryInformation.count;
            for( int i = 0; i < num_of_record; i += count_perrun ) {
                while( true ) {
                    try {
                        response = response.Concat( getChangedSupplierMeters( term, i, Math.Min( i + count_perrun - 1, num_of_record ), listType ).body.changedSupplierMeterListResponse ).ToList();

                        break;
                    } catch( Exception ex ) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw new Exception( "error on getting meter data configuration" );
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
                if( response.IndexOf( "SECURITYERROR" ) != -1 ) {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                } else {
                    return new JavaScriptSerializer().Deserialize<ChangedSupplierMeterServiceResponse>( response );
                }
            } else {
                return new ChangedSupplierMeterServiceResponse();
            }
        }

        /**
         * List Deducted Meters Service
         **/
        public List<DeductedMeterResponse> GetDeductedMetersRequest( DateTime term ) {
            List<DeductedMeterResponse> response = new List<DeductedMeterResponse>();
            DeductedMeterServiceResponse partial;

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    partial = GetDeductedMetersRequest( term, 0, 1 );

                    break;
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw new Exception( "error on getting meter data configuration" );
                    }
                }
            }

            /**
             * get all records.
             **/
            int num_of_record = partial.body.queryInformation.count;
            for( int i = 0; i < num_of_record; i += count_perrun ) {
                while( true ) {
                    try {
                        response = response.Concat( GetDeductedMetersRequest( term, i, Math.Min( i + count_perrun - 1, num_of_record ) ).body.deductedMeterListResponse ).ToList();

                        break;
                    } catch( Exception ex ) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw new Exception( "error on getting meter data configuration" );
                        }
                    }
                }
            }

            return response;
        }

        private DeductedMeterServiceResponse GetDeductedMetersRequest( DateTime term, int range_begin, int range_end ) {
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
                if( response.IndexOf( "body" ) == -1 ) {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                } else {
                    return new JavaScriptSerializer().Deserialize<DeductedMeterServiceResponse>( response );
                }
            } else {
                return new DeductedMeterServiceResponse();
            }
        }

        /**
         * List Meter Counts
         **/
        public List<ReturnedToSupplierMeterResponse> GetMeterCountRequest( DateTime term, string countType ) {
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
                        if( response.IndexOf( "body" ) == -1 ) {
                            throw new EXISTException( "" ) {
                                error = new JavaScriptSerializer().Deserialize<responseError>( response )
                            };
                        } else {
                            return ( new JavaScriptSerializer().Deserialize<MeterCountServiceResponse>( response ) ).body.meterCountResponseList;
                        }
                    } else {
                        return new List<ReturnedToSupplierMeterResponse>();
                    }
                } catch( EXISTException ex) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw new Exception( "error on getting meter data configuration" );
                    }
                }
            }
        }

        /**
         * Meter EIC Querying Service
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
                        if( response.IndexOf( "SECURE" ) != -1 ) {
                            throw new EXISTException( "" ) {
                                error = new JavaScriptSerializer().Deserialize<responseError>( response )
                            };
                        } else {
                            return ( new JavaScriptSerializer().Deserialize<MeteringPointEICQueryResponse>( response ) ).body.eicQueryResponseDatas;
                        }
                    } else {
                        return new List<MeteringPointEICQueryResponseData>();
                    }
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw new Exception( "error on getting meter data configuration" );
                    }
                }
            }
        }

        /**
         * 
         **/
        public List<MeterEicInfoResponse> GetMeterEicRequest( DateTime term ) {
            List<MeterEicInfoResponse> response = new List<MeterEicInfoResponse>();
            MeterEicInfoServiceResponse partial;

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    partial = GetMeterEicRequest( term, 0, 1 );

                    break;
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw new Exception( "error on getting meter data configuration" );
                    }
                }
            }

            /**
             * get all records.
             **/
            int num_of_record = partial.body.queryInformation.count;
            for( int i = 0; i < num_of_record; i += count_perrun ) {
                while( true ) {
                    try {
                        response = response.Concat( GetMeterEicRequest( term, i, Math.Min( i + count_perrun - 1, num_of_record ) ).body.meteringPointListResponse ).ToList();

                        break;
                    } catch( Exception ex ) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw new Exception( "error on getting meter data configuration" );
                        }
                    }
                }
            }

            return response;
        }

        public MeterEicInfoServiceResponse GetMeterEicRequest( DateTime meterEffectiveDate, int range_begin, int range_end ) {
            string request = ( new GetMeterEicRequest() {
                header = new List<Header> {
                    new Header("transactionId", Guid.NewGuid().ToString()),
                    new Header("application", "proGEDIA EXIST")
                },
                body = new ListMeterEicRequest() {
                    meterEffectiveDate = meterEffectiveDate,
                    range = new Range() {
                        begin = range_begin,
                        end = range_end
                    }
                }
            } ).ToString();

            string response = postRequest( request, url_tys + "/" + url_ddm );
            if( response.Length != 0 ) {
                if( response.IndexOf( "body" ) == -1 ) {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                } else {
                    return new JavaScriptSerializer().Deserialize<MeterEicInfoServiceResponse>( response );
                }
            } else {
                return new MeterEicInfoServiceResponse();
            }
        }
        /**
         * Service to control meter is read or not and Listing past meters 
         **/
        public List<MeterDatas> getMeterDataConfiguration( DateTime term, bool version = false ) {
            List<MeterDatas> response = new List<MeterDatas>();
            MeteringDataAndConfigurationQueryResponse partial;

            /**
             * get number of total record.
             **/
            while( true ) {
                try {
                    partial = getMeterDataConfiguration( term, 0, 1, version );

                    break;
                } catch( EXISTException ex ) {
                    throw ex;
                } catch( Exception ex ) {
                    if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                        throw new Exception( "error on getting meter data configuration" );
                    }
                }
            }

            /**
             * get all records.
             **/
            int num_of_record = partial.body.queryInformation.count;
            for( int i = 0; i < num_of_record; i += count_perrun ) {
                while( true ) {
                    try {
                        response = response.Concat( getMeterDataConfiguration( term, i, Math.Min( i + count_perrun - 1, num_of_record ), version ).body.meterDatas ).ToList();

                        break;
                    } catch( Exception ex ) {
                        if( insane_mode == false || ex.Message != "The operation has timed out" ) {
                            throw new Exception( "error on getting meter data configuration" );
                        }
                    }
                }
            }

            return response;
        }

        private MeteringDataAndConfigurationQueryResponse getMeterDataConfiguration( DateTime term, int range_begin, int range_end, bool pastVersion ) {
            if( getST() == false ) {
                return new MeteringDataAndConfigurationQueryResponse();
            }

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
                if( response.IndexOf( "SECURITYERROR" ) != -1 ) {
                    throw new EXISTException( "" ) {
                        error = new JavaScriptSerializer().Deserialize<responseError>( response )
                    };
                } else {
                    return new JavaScriptSerializer().Deserialize<MeteringDataAndConfigurationQueryResponse>( response );
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
                if( ex.Message.IndexOf( "Bad Request" ) != -1 ) {
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
     * response
     **/
    public class ChangedSupplierMeterServiceResponse {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public string resultType { get; set; }
        public ChangedSupplierMeterListResponse body { get; set; }
    }

    public class ChangedSupplierMeterResponse {
        public int newMeterId { get; set; }
        public string newMeterEic { get; set; }
        public string newOrganizationEic { get; set; }
        public string oldOrganizationEic { get; set; }
        public string newCustomerNo { get; set; }
        public string newMeterName { get; set; }
        public string newMeterAddress { get; set; }
        public int newMeterCountyId { get; set; }
        public int newMeterReadingType { get; set; }
        public string newMeterReadingTypeEnum { get; set; }
        public int newProfileSubscriptionGroup { get; set; }
        public double newAverageAnnualConsumption { get; set; }
        public string newDistributionMeterCode { get; set; }
        public string newprofileSubscriptionGroupName { get; set; }
        public string newCity { get; set; }
        public string oldOrganizationCode { get; set; }
        public string newOrganizationCode { get; set; }
    }

    public class ChangedSupplierMeterListResponse {
        public QueryInformation queryInformation { get; set; }
        public List<ChangedSupplierMeterResponse> changedSupplierMeterListResponse { get; set; }
    }

    /**
     * request
     **/
    public class GetChangedSupplierMetersRequest {
        public List<Header> header { get; set; }
        public ListChangedSupplierMeters body { get; set; }

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    public class ListChangedSupplierMeters {
        public DateTime term { get; set; }
        public string listType { get; set; }
        public Range range { get; set; }

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"listType\":\"" + listType + "\",\"range\":" + range.ToString() + "}";
        }
    }

    /**
     * list-deducted-meters
     **/
    /**
     * response
     **/
    public class DeductedMeterResponse {
        public int meterId { get; set; }
        public DateTime meterEffectiveDate { get; set; }
        public string meterEic { get; set; }
        public string city { get; set; }
        public object meterSerialNo { get; set; }
        public int customerNo { get; set; }
        public string meterName { get; set; }
        public int settlementPointId { get; set; }
        public string settlementPointName { get; set; }
    }

    public class DeductedMeterListResponse {
        public QueryInformation queryInformation { get; set; }
        public List<DeductedMeterResponse> deductedMeterListResponse { get; set; }
    }

    public class DeductedMeterServiceResponse {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public string resultType { get; set; }
        public DeductedMeterListResponse body { get; set; }
    }

    /**
     * request
     **/
    public class GetDeductedMetersRequest {
        public List<Header> header { get; set; }
        public ListDeductedMetersRequest body { get; set; }

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    public class ListDeductedMetersRequest {
        public DateTime term { get; set; }
        public Range range { get; set; }

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"range\":" + range.ToString() + "}";
        }
    }

    /**
     * list-meter-count
     **/
    /*
     * response
     **/
    public class ReturnedToSupplierMeterResponse {
        public object meterEffectiveDate { get; set; }
        public string readingType { get; set; }
        public int meterCount { get; set; }
    }

    public class ResponseBody {
        public List<ReturnedToSupplierMeterResponse> meterCountResponseList { get; set; }
    }

    public class MeterCountServiceResponse {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public string resultType { get; set; }
        public ResponseBody body { get; set; }
    }

    /*
     * request
     **/
    public class ListMeterCountRequest {
        public DateTime term { get; set; }
        public string countType { get; set; }

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"countType\":\"" + countType + "\"}";
        }
    }

    public class GetMeterCountRequest {
        public List<Header> header { get; set; }
        public ListMeterCountRequest body { get; set; }

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /**
     * list-meter-eic
     **/
    /*
     * response
     **/
    public class MeteringPointEICQueryResponseData {
        public string meterEic { get; set; }
        public string distributionMeterId { get; set; }
        public string customerNo { get; set; }
        public string eligibleConsumptionType { get; set; }
        public string meterUsageState { get; set; }
        public string supplierType { get; set; }
        public string meteringPointName { get; set; }
        public string meteringAddress { get; set; }
        public int? cityId { get; set; }
        public int? countyId { get; set; }
        public string meterReadingCompanyId { get; set; }
        public string meterReadingCompanyEic { get; set; }
        public string status { get; set; }
        public string description { get; set; }
    }

    public class MeteringPointEICQueryResponseDataList {
        public List<MeteringPointEICQueryResponseData> eicQueryResponseDatas { get; set; }
    }

    public class MeteringPointEICQueryResponse {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public string resultType { get; set; }
        public MeteringPointEICQueryResponseDataList body { get; set; }
    }

    /*
     * request
     **/
    public class MeteringPointEICQuery {
        public string meterEic { get; set; }
        public string distributionMeterCode { get; set; }
        public string meterReadingCompanyEic { get; set; }

        public override string ToString( ) {
            return "{\"meterEic\":\"" + meterEic + "\",\"distributionMeterCode\":\"" + distributionMeterCode + "\",\"meterReadingCompanyEic\":\"" + meterReadingCompanyEic + "\"}";
        }
    }

    public class MeteringPointEICQueryList {
        public List<MeteringPointEICQuery> meteringPointEICQueries { get; set; }

        public override string ToString( ) {
            return "{\"meteringPointEICQueries\":[" + string.Join<MeteringPointEICQuery>( ",", meteringPointEICQueries.ToArray() ) + "]}";
        }
    }

    public class MeteringPointEICQueryRequest {
        public List<Header> header { get; set; }
        public MeteringPointEICQueryList body { get; set; }

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /**
     * list-meter-eic-range
     **/
    /*
     * response
     **/
    public class MeterEicInfoResponse {
        public int id { get; set; }
        public string meterEic { get; set; }
        public string distributionMeterId { get; set; }
        public bool status { get; set; }
        public int meterReadingCompany { get; set; }
        public string withDrawalDeducSettlementPointEic { get; set; }
        public string meterName { get; set; }
        public string meterAddress { get; set; }
        public int countyId { get; set; }
        public double transformerInputVoltage { get; set; }
        public double transformerOutputVoltage { get; set; }
        public string customerNo { get; set; }
        public int substation { get; set; }
        public int connectionPointLocation { get; set; }
        public string connectionPointLocationOther { get; set; }
        public int meteringVoltage { get; set; }
        public int? profileType { get; set; }
        public int? profileSubscriptionGroup { get; set; }
        public int readingType { get; set; }
        public double transformerPower { get; set; }
        public int busbarVoltage { get; set; }
        public int noLoadLoss { get; set; }
        public int loadLoss { get; set; }
        public object temperatureCoefficient { get; set; }
        public object lineLength { get; set; }
        public object lineSection { get; set; }
        public object lineCircuit { get; set; }
        public bool transLossFactorStatus { get; set; }
        public double averageAnnualConsumption { get; set; }
        public object supplyPosition { get; set; }
        public int withdrawalPosition { get; set; }
        public object addressCode { get; set; }
        public bool zonningPosition { get; set; }
        public int usageState { get; set; }
        public object organizedIndustrialZoneEic { get; set; }
        public bool canLoadProfile { get; set; }
        public object mainEligibleConsumptionEic { get; set; }
        public object supplyDeductionSettlementPoint { get; set; }
        public int eligibleConsumptionType { get; set; }
        public bool amr { get; set; }
        public object maxAnnualConsumption { get; set; }
        public bool estimation { get; set; }
        public string withdrawalPositionDescription { get; set; }
        public string serialNumber { get; set; }
        public string manufacturer { get; set; }
        public string supplierOrganization { get; set; }
        public object contractPower { get; set; }
        public object tariffClass { get; set; }
        public object mainTariffGroup { get; set; }
        public object activityCode { get; set; }
        public int meteringType { get; set; }
        public int supplierType { get; set; }
        public string countyName { get; set; }
        public object organizedIndustrialZone { get; set; }
        public int meteringVoltageValue { get; set; }
        public int busbarVoltageValue { get; set; }
        public object lineCircuitDesc { get; set; }
        public object temperatureCoefficientValue { get; set; }
        public string profileSubscriptionGroupDesc { get; set; }
        public string profileTypeDesc { get; set; }
        public string withdrawalPositionName { get; set; }
        public object supplyPositionName { get; set; }
        public string eligibleConsumptionTypeDesc { get; set; }
        public string usageStateDesc { get; set; }
        public string supplierTypeDesc { get; set; }
        public string connectionPointLocationDesc { get; set; }
        public string substationDesc { get; set; }
        public DateTime registrationDate { get; set; }
    }

    public class MeterEicInfoListResponse {
        public QueryInformation queryInformation { get; set; }
        public List<MeterEicInfoResponse> meteringPointListResponse { get; set; }
    }

    public class MeterEicInfoServiceResponse {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public string resultType { get; set; }
        public MeterEicInfoListResponse body { get; set; }
    }

    /*
     * request
     **/
    public class ListMeterEicRequest {
        public string meterEIC { get; set; }
        public string eligibleConsumptionType { get; set; }
        public DateTime meterEffectiveDate { get; set; }
        public string meterId { get; set; }
        public string meterUsageState { get; set; }
        public string supplierType { get; set; }
        public Range range { get; set; }

        public override string ToString( ) {
            return "{\"meterEic\":\"" + meterEIC + "\",\"eligibleConsumptionType\":\"" + eligibleConsumptionType + "\",\"meterEffectiveDate\":\"" + proGEDIA.ToString( meterEffectiveDate ) + "\",\"meterId\":\"" + meterId + "\",\"meterUsageState\":\"" + meterUsageState + "\",\"supplierType\":\"" + supplierType + "\",\"range\":" + range.ToString() + "}";
        }
    }

    public class GetMeterEicRequest {
        public List<Header> header { get; set; }
        public ListMeterEicRequest body { get; set; }

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    /**
     * list-meter-data-configuration
     **/
    /**
     * response
     **/
    public class MeteringDataAndConfigurationQueryResponse {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public string resultType { get; set; }
        public MeteringDataAndConfigurationList body { get; set; }
    }

    public class MeteringDataAndConfigurationList {
        public QueryInformation queryInformation { get; set; }
        public List<MeterDatas> meterDatas { get; set; }
    }

    public class MeterDatas {
        public UInt32 meterId { get; set; }
        public string meterEic { get; set; }
        public string meterCity { get; set; }
        public DateTime? term { get; set; }
        public DateTime? dataVersion { get; set; }
        public DateTime? confVersion { get; set; }
        public string meterConsumption { get; set; }
        public string meterGeneration { get; set; }
        public string meterLossyConsumption { get; set; }
        public string meterLossyGeneration { get; set; }
        public string readingType { get; set; }
        public string supplierOrganization { get; set; }
        public string meterReadingCompany { get; set; }
        public bool isConfWithdrawalSettlement { get; set; }
        public bool isConfSupplySettlement { get; set; }
        public bool isConfWithdDeducSettlement { get; set; }
        public bool isConfSupplyDeducSettlement { get; set; }
        public bool isRead { get; set; }
    }   

    /**
     * request
     **/
    public class MeteringDataConfigurationQueryRequest {
        public List<Header> header;
        public MeteringDataConfigurationQuery body;

        public override string ToString( ) {
            return "{\"header\":[" + string.Join<Header>( ",", header.ToArray() ) + "],\"body\":" + body.ToString() + "}";
        }
    }

    public class MeteringDataConfigurationQuery {
        private string _meteringReadingType;

        public DateTime term;
        public string meteringReadingType {
            get { return ( _meteringReadingType == "null" ) ? ( "null" ) : ( "\"" + _meteringReadingType + "\"" ); }
            set { this._meteringReadingType = value; }
        }
        public bool pastVersion;
        public Range range;

        public override string ToString( ) {
            return "{\"term\":\"" + proGEDIA.ToString( term ) + "\",\"pastVersion\":" + meteringReadingType + ",\"range\":" + range.ToString() + "}";
        }
    }

    /**
     * Common classes
     **/
    public class QueryInformation {
        public int begin { get; set; }
        public int end { get; set; }
        public int count { get; set; }
    }

    public class Header {
        public string key;
        public string value;

        public Header( string key, string value ) {
            this.key = key;
            this.value = value;
        }

        public override string ToString( ) {
            return "{\"key\":\"" + key + "\",\"value\":\"" + value + "\"}";
        }
    }

    public class Range {
        public int begin;
        public int end;

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

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- TABLE: STATUS CODE TABLE
-- A CODE TABLE CONTAINING THE ALLOWED STATUS CODES FOR USE IN THE AUDIT SUBSYSTEM
CREATE TABLE aud_sts_cdtbl
(
	cd_id	NUMERIC(1) NOT NULL, -- THE CODE IDENTIFIER
	name	VARCHAR(16) NOT NULL UNIQUE, -- THE DISPLAY NAME OF THE CODE
	CONSTRAINT pk_aud_sts_cdtbl PRIMARY KEY (cd_id)
);

INSERT INTO aud_sts_cdtbl VALUES (0, 'NEW'); -- DATA IS NEW AND UNVERIFIED
INSERT INTO aud_sts_cdtbl VALUES (1, 'ACTIVE'); -- DATA IS VERIFIED AND ACTIVE
INSERT INTO aud_sts_cdtbl VALUES (2, 'HELD'); -- DATA WAS ACTIVE BUT IS NOW ON HOLD AND REQUIRES A REVIEW
INSERT INTO aud_sts_cdtbl VALUES (3, 'NULLIFIED'); -- DATA IS NULLIFIED AND WAS NEVER INTENDED TO BE ENTERED
INSERT INTO aud_sts_cdtbl VALUES (4, 'OBSOLETE'); -- DATA IS OBSOLETE AND HAS BEEN REPLACED
INSERT INTO aud_sts_cdtbl VALUES (5, 'ARCHIVED'); -- DATA WAS ACTIVE BUT IS NO LONGER RELEVANT
INSERT INTO aud_sts_cdtbl VALUES (6, 'SYSTEM'); -- SYSTEM LEVEL AUDIT NOT FOR DISPLAY 

-- TABLE: node_tbl TABLE 
-- TRACKS THE PHYSICAL DEVICES (node_tblS) THAT ARE PERMITTED TO USE THE SYSTEM
CREATE TABLE aud_node_tbl
(
	node_id UUID NOT NULL DEFAULT uuid_generate_v1(),
	dev_id UUID, -- THE DEVICE IDENTIFIER 
	name VARCHAR(256), -- A FRIENDLY NAME FOR THE node_tbl
	host_name VARCHAR(256), -- THE HOST URL/SCHEME
	sts_cd_id INTEGER NOT NULL DEFAULT 0, -- THE STATUS OF THE node_tbl
	crt_utc TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, -- THE CREATION TIME OF THIS VERSION
	crt_usr_id UUID NOT NULL, -- THE USER THAT CREATED THE NODE
	upd_utc TIMESTAMPTZ, -- THE TIME THE NODE WAS CHANGED
	upd_usr_id UUID, -- THE USER THAT UPDATED THE NODE
	obslt_utc TIMESTAMPTZ, -- THE TIME THIS RECORD WAS OBSOLETED	CONSTRAINT pk_node_tbl PRIMARY KEY (node_id)
	obslt_usr_id UUID, -- THE USER THAT OBSOLETED THE DATA
	CONSTRAINT pk_aud_node_tbl PRIMARY KEY (node_id),
	CONSTRAINT fk_aud_node_tbl_sts_cd_id FOREIGN KEY (sts_cd_id) REFERENCES aud_sts_cdtbl(cd_id)
);

-- TABLE: AUDIT CODE TABLE
-- USED TO TRACK THE CODES THAT ARE USED IN THE AUDITING
CREATE TABLE aud_cd_tbl
(
	cd_id UUID NOT NULL DEFAULT uuid_generate_v1(), -- A UNIQUE ID FOR THE CODE
	mnemonic VARCHAR(54) NOT NULL, -- THE MNEMNONIC FOR THE CODE
	domain VARCHAR(54) NOT NULL, -- A domain TO WHICH THE mnemonic BELONGS
	display VARCHAR(256), -- THE HUMAN READABLE NAME FOR THE CODE
	CONSTRAINT pk_aud_cd_tbl PRIMARY KEY (cd_id)
);
CREATE UNIQUE INDEX aud_cd_mnemonic_idx ON aud_cd_tbl(domain, mnemonic);

-- SEED DATA
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('1','AuditableObjectType','Person');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('2','AuditableObjectType','System Object');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('3','AuditableObjectType','Organization');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('4','AuditableObjectType','Other');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('1','AuditableObjectRole','Patient');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('2','AuditableObjectRole','Location');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('3','AuditableObjectRole','Report');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('4','AuditableObjectRole','Resource');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('5','AuditableObjectRole','Master File');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('6','AuditableObjectRole','User');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('7','AuditableObjectRole','List');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('8','AuditableObjectRole','Doctor');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('9','AuditableObjectRole','Subscriber');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('10','AuditableObjectRole','Guarantor');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('11','AuditableObjectRole','Security User');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('12','AuditableObjectRole','Security Group');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('13','AuditableObjectRole','Security Resource');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('14','AuditableObjectRole','Security Granularity Definition');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('15','AuditableObjectRole','Provider');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('16','AuditableObjectRole','Data Destination');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('17','AuditableObjectRole','Data Repository');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('18','AuditableObjectRole','Schedule');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('19','AuditableObjectRole','Customer');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('20','AuditableObjectRole','Job');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('21','AuditableObjectRole','Job Stream');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('22','AuditableObjectRole','Table');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('23','AuditableObjectRole','Routing Criteria');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('24','AuditableObjectRole','Query');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('1','AuditableObjectLifecycle','Creation');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('2','AuditableObjectLifecycle','Import');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('3','AuditableObjectLifecycle','Amendment');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('4','AuditableObjectLifecycle','Verification');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('5','AuditableObjectLifecycle','Translation');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('6','AuditableObjectLifecycle','Access');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('7','AuditableObjectLifecycle','Deidentification');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('8','AuditableObjectLifecycle','Aggregation');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('9','AuditableObjectLifecycle','Report');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('10','AuditableObjectLifecycle','Export');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('11','AuditableObjectLifecycle','Disclosure');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('12','AuditableObjectLifecycle','Receipt of Disclosure');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('13','AuditableObjectLifecycle','Archiving');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('14','AuditableObjectLifecycle','Logical Deletion');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('15','AuditableObjectLifecycle','Permanent Erasure');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('1','RFC-3881','Medical Record');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('2','RFC-3881','Patient Number');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('3','RFC-3881','Encounter Number');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('4','RFC-3881','Enrollee Number');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('5','RFC-3881','Social Security Number');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('6','RFC-3881','Account Number');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('7','RFC-3881','Guarantor Number');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('8','RFC-3881','Report Name');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('9','RFC-3881','Report Number');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('10','RFC-3881','Search Criterion');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('11','RFC-3881','User Identifier');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('12','RFC-3881','Uri');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:a54d6aa5-d40d-43f9-88c5-b4633d873bdd','IHE XDS Meta Data','Submission Set');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:a7058bb9-b4e4-4307-ba5b-e3f0ab85e12d','IHE XDS Meta Data','Submission Set Author');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:aa543740-bdda-424e-8c96-df4873be8500','IHE XDS Meta Data','Submission Set Content Type');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:96fdda7c-d067-4183-912e-bf5ee74998a8','IHE XDS Meta Data','Submission Set Unique Id');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:7edca82f-054d-47f2-a032-9b2a5b5186c1','IHE XDS Meta Data','Document Entry');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:93606bcf-9494-43ec-9b4e-a7748d1a838d','IHE XDS Meta Data','Document Entry Author');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:41a5887f-8865-4c09-adf7-e362475b143a','IHE XDS Meta Data','Document Entry Class Code');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:f4f85eac-e6cb-4883-b524-f2705394840f','IHE XDS Meta Data','Document Entry Confidentiality Code');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:2c6b8cb7-8b2a-4051-b291-b1ae6a575ef4','IHE XDS Meta Data','Document Entry Event Code List');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:a09d5840-386c-46f2-b5ad-9c3699a4309d','IHE XDS Meta Data','Document Entry Format Code');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:f33fb8ac-18af-42cc-ae0e-ed0b0bdb91e1','IHE XDS Meta Data','Document Entry Health Care Fcility Type');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:58a6f841-87b3-4a3e-92fd-a8ffeff98427','IHE XDS Meta Data','Document Entry Patient Id');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:cccf5598-8b07-4b77-a05e-ae952c785ead','IHE XDS Meta Data','Document Entry Practice Setting');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:f0306f51-975f-434e-a61c-c59651d33983','IHE XDS Meta Data','Document Entry Type Code');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:2e82c1f6-a085-4c72-9da3-8640a32e42ab','IHE XDS Meta Data','Document Entry Unique Id');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:d9d542f3-6cc4-48b6-8870-ea235fbc94c2','IHE XDS Meta Data','Folder');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:1ba97051-7806-41a8-a48b-8fce7af683c5','IHE XDS Meta Data','Folder Code List');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:f64ffdf0-4b97-4e06-b79f-a52b38ec2f8a','IHE XDS Meta Data','Folder Patient Id');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:75df8f67-9973-4fbe-a900-df66cefecc5a','IHE XDS Meta Data','Folder Unique Id');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:917dc511-f7da-4417-8664-de25b34d3def','IHE XDS Meta Data','Append');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:60fd13eb-b8f6-4f11-8f28-9ee000184339','IHE XDS Meta Data','Replacement');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:ede379e6-1147-4374-a943-8fcdcf1cd620','IHE XDS Meta Data','Transform');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:b76a27c7-af3c-4319-ba4c-b90c1dc45408','IHE XDS Meta Data','Transform / Replace');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:8ea93462-ad05-4cdc-8e54-a8084f6aff94','IHE XDS Meta Data','Sign');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('urn:uuid:10aa1a4b-715a-4120-bfd0-9760414112c8','IHE XDS Meta Data','Document Entry Stub');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-1','IHE Transactions','Maintain Time');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-2','IHE Transactions','Get User Authentication');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-3','IHE Transactions','Get Service Ticket');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-4','IHE Transactions','Kerberized Communication');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-5','IHE Transactions','Join Context');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-6','IHE Transactions','Change Context');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-7','IHE Transactions','Leave Context');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-8','IHE Transactions','Patient Identity Feed');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-9','IHE Transactions','PIX Query');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-10','IHE Transactions','PIX Update Notification');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-11','IHE Transactions','Retrieve Specific Information for Display');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-12','IHE Transactions','Retrieve Document for Display');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-13','IHE Transactions','Follow Context');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-14','IHE Transactions','Register Document Set');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-15','IHE Transactions','Provide and Register Document Set');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-16','IHE Transactions','Query Registry');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-17','IHE Transactions','Retrieve Documents');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-18','IHE Transactions','Registry Stored Query');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-19','IHE Transactions','Authenticate node_tbl');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-20','IHE Transactions','Record Audit Event');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-21','IHE Transactions','Patient Demographics Query');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-22','IHE Transactions','Patient Demographics and Visit Query');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-23','IHE Transactions','Find Personnel White Pages');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-24','IHE Transactions','Query Personnel White Pages');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-30','IHE Transactions','Patient Identity Management');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-31','IHE Transactions','Patient Encounter Management');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-32','IHE Transactions','Distribute Document Set on Media');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-38','IHE Transactions','Cross Gateway Query');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-39','IHE Transactions','Cross Gateway Retrieve');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-40','IHE Transactions','Provide X-User Assertion');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-41','IHE Transactions','Provide and Register Document Set-b');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-42','IHE Transactions','Register Document Set-b');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-43','IHE Transactions','Retrieve Document Set');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-44','IHE Transactions','Patient Identity Feed HL7v3');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-45','IHE Transactions','PIXv3 Query');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-46','IHE Transactions','PIXv3 Update Notification');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-47','IHE Transactions','Patient Demographics Query HL7v3');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('ITI-51','IHE Transactions','Multi-Patient Stored Query');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('1','NetworkAccessPointType','Machine Name');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('2','NetworkAccessPointType','IP Address');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('3','NetworkAccessPointType','Telephone Number');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('4','NetworkAccessPointType','Email Address');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('5','NetworkAccessPointType','URI');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('C','ActionType','Create');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('R','ActionType','Read');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('U','ActionType','Update');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('D','ActionType','Delete');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('E','ActionType','Execute');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('0','OutcomeIndicator','Success');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('4','OutcomeIndicator','Minor Fail');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('8','OutcomeIndicator','Serious Fail');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('12','OutcomeIndicator','Major Fail');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('IHE0001','IHE','Provisioning Event');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('IHE0002','IHE','Medication Event');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('IHE0003','IHE','Resource Assignment');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('IHE0004','IHE','Care Episode');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('IHE0005','IHE','Care Protocol');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('IHE0006','IHE','Disclosure');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('CDT-100002','CDT', 'Patient Search Activity');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110100','DCM','Application Activity');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110101','DCM','Audit Log Used');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110102','DCM','Begin Transferring DICOM Instances ');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110103','DCM','DICOM Instances Accessed');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110104','DCM','DICOM Instances Transferred');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110105','DCM','DICOM Study Deleted');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110106','DCM','Export');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110107','DCM','Import');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110108','DCM','Network Activity');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110109','DCM','Order Record');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110110','DCM','Patient Record');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110111','DCM','Procedure Record');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110112','DCM','Query');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110113','DCM','Security Alert');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110114','DCM','User Authentication');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110120','DCM','Application Start');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110121','DCM','Application Stop');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110122','DCM','Login');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110123','DCM','Logout');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110124','DCM','Attach');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110125','DCM','Detach');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110126','DCM','node_tbl Authentication');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110127','DCM','Emergency Override Started');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110132','DCM','Use of a restricted function');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110135','DCM','Object Security Attributes Changed');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110136','DCM','Security Roles Changed');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110137','DCM','User security Attributes Changed');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110153','DCM','Source');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('110152','DCM','Destination');

INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('1','AuditSourceType','End User Interface');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('2','AuditSourceType','Device Or Instrument');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('3','AuditSourceType','Web Server Process');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('4','AuditSourceType','Application Server Process');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('5','AuditSourceType','Database Server Process');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('6','AuditSourceType','Security Service Process');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('7','AuditSourceType','ISO Level 1 or 3 Component');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('8','AuditSourceType','ISO Level 4 or 6 Software');
INSERT INTO aud_cd_tbl (mnemonic, domain, display) VALUES ('9','AuditSourceType','Other');

-- TABLE: AUDIT SESSION
-- TRACKS A SESSION WITH A REMOTE HOST FOR LONG RUNNING AUDITS OR BATCHES OF AUDIT SUBMISSIONS
CREATE TABLE aud_ses_tbl
(
	ses_id UUID NOT NULL, -- UNIQUE IDENTIFIER FOR THE SESSION
	rcv_node_id UUID NOT NULL, -- THE ID OF THE RECEIVING node_tbl
	rcv_ep VARCHAR(256) NOT NULL, -- THE ENDPOINT THE AUDIT WAS RECCEIVED ON
	snd_node_id UUID NOT NULL, -- THE ID OF THE SENDING node_tbl
	snt_ep VARCHAR(256) NOT NULL, -- THE SENDING ENDPOINT
	crt_utc TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, -- THE TIME THAT THE SESSION WAS CREATED
	crt_usr_id UUID NOT NULL, 
	obslt_utc TIMESTAMPTZ,
	obslt_usr_id UUID,
	CONSTRAINT pk_aud_ses_tbl PRIMARY KEY (ses_id),
	CONSTRAINT fk_rcvr_nod_id FOREIGN KEY (rcv_node_id) REFERENCES aud_node_tbl(node_id),
	CONSTRAINT fk_snd_node_id FOREIGN KEY (snd_node_id) REFERENCES aud_node_tbl(node_id)
);

-- SEQUENCE
CREATE SEQUENCE aud_seq START WITH 1 INCREMENT BY 1;

-- TABLE: AUDIT TABLE
-- TRACKS THE AUDITS SENT TO THE VISUALIZER
CREATE TABLE aud_tbl
(
	aud_id	UUID NOT NULL DEFAULT uuid_generate_v1(), -- UNIQUE IDENTIFIER FOR THE AUDIT 
	aud_seq_id NUMERIC(20,0) NOT NULL DEFAULT nextval('aud_seq'), -- SEQUENCE OF THE AUDIT
	corr_id UUID NOT NULL, -- CORRELATION IDENTIFIER FOR THE AUDIT FROM THE EXTERNAL SYSTEM 
	act_cd_id UUID NOT NULL, -- THE CODE CONTAINING THE ACTION
	out_cd_id UUID NOT NULL, -- THE CODE CONTAINING THE OUTCOME
	evt_cd_id UUID NOT NULL, -- THE EVENT CODE
	evt_utc TIMESTAMPTZ NOT NULL, -- THE TIME THE EVENT OCCURRED
	ses_id UUID, -- THE UUID OF THE AUDIT CORRELATION
	ps_name VARCHAR(256), -- THE NAME OF THE PROCESS
	ps_id VARCHAR(256) NOT NULL, 
	CONSTRAINT pk_aud_tbl PRIMARY KEY (aud_id),
	CONSTRAINT fk_aud_act_cd_id_tbl FOREIGN KEY (act_cd_id) REFERENCES aud_cd_tbl(cd_id),
	CONSTRAINT fk_aud_out_cd_id_tbl FOREIGN KEY (out_cd_id) REFERENCES aud_cd_tbl(cd_id),
	CONSTRAINT fk_aud_evt_cd_id_tbl FOREIGN KEY (evt_cd_id) REFERENCES aud_cd_tbl(cd_id),
	CONSTRAINT fk_aud_ses_id_tbl FOREIGN KEY (ses_id) REFERENCES aud_ses_tbl(ses_id)
);

-- INDEX: LOOKUP BY ACTION CODE OR OUTCOME
CREATE INDEX aud_act_cd_idx ON aud_tbl(act_cd_id);
CREATE INDEX act_out_cd_idx ON aud_tbl(out_cd_id);


-- TABLE: TRACK AUDIT PARTICIPANTS
-- TRACKS THE PARTICIPANTS IN AN AUDIT
CREATE TABLE aud_ptcpt_tbl
(
	ptcpt_id UUID NOT NULL DEFAULT uuid_generate_v1(), -- UNIQUE ISENTIFIER FOR THE PARTICIPANT
	node_id UUID, -- THE LINKED NODE VERSION IN THE RECORD
	sid UUID, -- THE USER OR DEVICE SECURITY IDENTIFIER MAPPED TO 
	usr_id VARCHAR(256), -- THE USER IDENTIFIER AS IT APPEARED ON THE AUDIT
	usr_name VARCHAR(256), -- THE USER NAME AS IT APPEARED ON THE AUDIT
	net_ap VARCHAR(256), -- THE IP ADDRESS OF THE PARITICIPANT
	net_ap_typ INTEGER, 
	CONSTRAINT pk_aud_ptcpt_tbl PRIMARY KEY (ptcpt_id),
	CONSTRAINT fk_aud_ptcpt_node_tbl FOREIGN KEY (node_id) REFERENCES aud_node_tbl(node_id),
);

-- INDEX: LOOKUP PARTICIPANT BY AUDIT ID, USER ID or node_tbl VERSION
CREATE INDEX aud_ptcpt_usr_idx ON aud_ptcpt_tbl(usr_id);
CREATE INDEX aud_ptcpt_node_idx ON aud_ptcpt_tbl(node_id);

-- LINKS PARTICIPANTS TO AUDITS
CREATE TABLE aud_ptcpt_aud_assoc_tbl 
(
	assoc_id UUID NOT NULL DEFAULT uuid_generate_v1(), -- SURROGATE KEY
	aud_id UUID NOT NULL, -- AUDIT IDENTIFIER
	ptcpt_id UUID NOT NULL, -- PARTICIPANT IDENTIFIER 
	is_rqo BOOLEAN NOT NULL DEFAULT FALSE, -- TRUE IF THE PARTICIPANT INITIATED THE REQUEST
	CONSTRAINT pk_aud_ptcpt_aud_assoc_tbl PRIMARY KEY (assoc_id),
	CONSTRAINT fk_aud_ptcpt_aud_aud_tbl FOREIGN KEY (aud_id) REFERENCES aud_tbl(aud_id),
	CONSTRAINT fk_aud_ptcpt_aud_ptcpt_tbl FOREIGN KEY (ptcpt_id) REFERENCES aud_ptcpt_tbl(ptcpt_id)
);

-- TABLE: AUDIT PARTICIPANT ROLE CODE ASSOCIATION
-- TRACKS AN ASSOCIATION BETWEEN AN AUDIT ACTIVE PARTICIPANT AND THE ROLE CODES
CREATE TABLE aud_ptcpt_rol_cd_assoc_tbl
(
	assoc_id UUID NOT NULL DEFAULT uuid_generate_v1(), -- ASSOCIATION THIS ROLE APPLIES TO
	cd_id UUID NOT NULL, -- THE IDENTIFIER FOR THE ROLE CODE
	CONSTRAINT pk_aud_ptcpt_rol_cd_assoc_tbl PRIMARY KEY (assoc_id, cd_id),
	CONSTRAINT fk_aud_ptcpt_rol_cd_cd_tbl FOREIGN KEY (cd_id) REFERENCES aud_cd_tbl(cd_id)
);

-- INDEX: LOOKUP PARTICIPANT ROLE CODE BY PARTICIPANT ID
CREATE INDEX aud_ptcpt_rol_cd_assoc_ptcpt_idx ON aud_ptcpt_rol_cd_assoc_tbl(ptcpt_id);
CREATE INDEX aud_ptcpt_rol_cd_assoc_aud_idx ON aud_ptcpt_rol_cd_assoc_tbl(aud_id);


-- TABLE: AUDIT STATUS TABLE
-- TRACKS THE STATUS OF AN AUDIT OVER TIME
CREATE TABLE aud_vrsn_tbl
(
	aud_vrsn_id UUID NOT NULL DEFAULT uuid_generate_v1(), -- UNIQUE IDENTIFIER FOR THE CHANGE TO THE AUDIT STATUS
	aud_id UUID NOT NULL, -- THE IDENTIFIER OF THE AUDIT FOR WHICH THIS CHANGE APPLIES
	rplc_vrsn_id UUID NOT NULL, -- THE VERSION THAT THIS RECORD REPLACES
	sts_cd_id INTEGER NOT NULL, -- THE STATUS CODE TO WHICH THE NEW AUDIT IS TARGETED
	is_alrt BOOLEAN DEFAULT FALSE,
	crt_utc TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, -- THE TIME THE STATUS VERSION WAS CREATED
	crt_usr_id UUID NOT NULL, -- THE USER THAT RESULTED IN THE CREATION OF THIS VERSION
	obslt_utc TIMESTAMPTZ, -- THE TIME WHEN THE STATUS WAS NO LONGER VALID
	obslt_usr_id UUID, -- OBSOLETION USER IDENTIFIER
	CONSTRAINT pk_aud_vrsn_tbl PRIMARY KEY (aud_vrsn_id),
	CONSTRAINT fk_aud_vrsn_aud_tbl FOREIGN KEY (aud_id) REFERENCES aud_tbl(aud_id),
	CONSTRAINT fk_aud_vrsn_sts_tbl FOREIGN KEY (sts_cd_id) REFERENCES aud_sts_cdtbl(cd_id)
);

-- INDEX: LOOKUP AUDIT STATUS BY AUDIT ID
CREATE INDEX aud_vrsn_aud_idx ON aud_vrsn_tbl(aud_id);

-- TABLE: AUDIT SOURCES TABLE
-- TRACKS AN AUDIT SOURCE'S ENTERPRISE SITE AND SOURCE
CREATE TABLE aud_src_tbl
(
	aud_src_id UUID NOT NULL DEFAULT uuid_generate_v1(), -- A UNIQUE IDENTIFIER FOR THE AUDIT SOURCE RECORD
	ent_ste_nam VARCHAR(256), -- THE NAME OF THE ENTERPRISE SITE
	aud_src_nam VARCHAR(256), -- THE NAME OF THE AUDIT SOURCE
	CONSTRAINT pk_aud_src_tbl PRIMARY KEY (aud_src_id)
);

-- TRACKS A RELATIONSHIP BETWEEN THE AUDIT SOURCE AND TYPES
CREATE TABLE aud_src_typ_tbl
(
	aud_src_id UUID NOT NULL, -- THE AUDIT SOURCE TO WHICH THE ASSOCIATION APPLIES
	cd_id UUID NOT NULL, -- THE CODE OF WHICH THE AUDIT SOURCE IS
	CONSTRAINT pk_aud_src_typ_tbl PRIMARY KEY(aud_src_id, cd_id),
	CONSTRAINT fk_aud_src_aud_src_tbl FOREIGN KEY (aud_src_id) REFERENCES aud_src_tbl(aud_src_id),
	CONSTRAINT fk_aud_src_cd_tbl FOREIGN KEY (cd_id) REFERENCES aud_cd_tbl(cd_id)
);

-- TABLE: AUDIT SOURCE TO AUDIT ASSOCIATION TABLE
-- TRACKS THE RELATIONSHIP BETWEEN AN AUDIT SOURCE AND AN AUDIT MESSAGE
CREATE TABLE aud_src_assoc_tbl
(
	assoc_id UUID NOT NULL DEFAULT uuid_generate_v1(),
	aud_src_id UUID NOT NULL, -- THE AUDIT SOURCE TO WHICH THE ASSOCIATION APPLIES,
	aud_id UUID NOT NULL, -- THE IDENTIFIER OF THE AUDIT TO WHICH THE ASSOCIATION APPLIES
	CONSTRAINT pk_aud_src_assoc_tbl PRIMARY KEY (assoc_id),
	CONSTRAINT fk_aud_src_assoc_aud_src_tbl FOREIGN KEY (aud_src_id) REFERENCES aud_src_tbl(aud_src_id),
	CONSTRAINT fk_aud_src_assoc_aud_tbl FOREIGN KEY (aud_id) REFERENCES aud_tbl(aud_id)
);

-- INDEX: LOOKUP AUDIT SOURCE BY AUDIT ID
CREATE INDEX aud_src_assoc_aud_idx ON aud_src_assoc_tbl(aud_id);


-- TABLE: AUDIT EVENT TYPE CODE ASSOCIATION TABLE
-- TRACKS THE RELATIONSHIP BETWEEN AN AUDIT AND TYPE CODES
CREATE TABLE aud_evt_typ_cd_assoc_tbl
(
	aud_id UUID NOT NULL, -- THE IDENTIFIER OF THE AUDIT TO WHICH THE ASSOCIATION APPLIES,
	cd_id UUID NOT NULL, -- THE IDENTIFIER OF THE CODE WHICH CARRIES MEANING IN RELATION TO THE EVENT TYPE
	CONSTRAINT pk_aud_evt_typ_cd_assoc_tbl PRIMARY KEY (aud_id, cd_id),
	CONSTRAINT fk_aud_evt_typ_cd_aud_tbl FOREIGN KEY (aud_id) REFERENCES aud_tbl(aud_id),
	CONSTRAINT fk_aud_evt_typ_cd_cd_tbl FOREIGN KEY (cd_id) REFERENCES aud_cd_tbl(cd_id)
);

-- INDEX: LOOKUP EVENT TYPE BY CODE OR AUDIT
CREATE INDEX aud_evt_typ_cd_assoc_cd_idx ON aud_evt_typ_cd_assoc_tbl(aud_id);

-- TABLE: AUDIT PARTICIPANT OBJECTS DETAIL
-- TRACKS DETAILS RELATED TO THE OBJECTS THAT WERE UPDATED, DISCLOSED, OR CREATED FOR THIS AUDIT
CREATE TABLE aud_obj_tbl 
(
	obj_id UUID NOT NULL DEFAULT uuid_generate_v1(), -- A UNIQUE IDENTIFIER FOR THE OBJECT
	aud_id UUID NOT NULL, -- THE AUDIT TO WHICH THE OBJECT APPLIES
	ext_id VARCHAR(256) NOT NULL, -- THE EXTERNAL IDENTIFIER (ID) OF THE PARTICIPANT OBJECT
	typ_cd_id UUID NOT NULL, -- THE TYPE OF OBJECT REFERENCED
	rol_cd_id UUID, -- THE ROLE OF THE OBJECT IN THE EVENT
	lcycl_cd_id UUID, -- THE LIFECYCLE OF THE OBJECT IF APPLICABLE
	id_typ_cd_id UUID, -- SPECIFIES THE TYPE CODE FOR THE OBJECT
	CONSTRAINT pk_aud_obj_tbl PRIMARY KEY (obj_id),
	CONSTRAINT fk_aud_obj_typ_cd_tbl FOREIGN KEY (typ_cd_id) REFERENCES aud_cd_tbl(cd_id),
	CONSTRAINT fk_aud_obj_lcycl_cd_tbl FOREIGN KEY (lcycl_cd_id) REFERENCES aud_cd_tbl(cd_id),
	CONSTRAINT fk_aud_obj_rol_cd_tbl FOREIGN KEY (rol_cd_id) REFERENCES aud_cd_tbl(cd_id),
	CONSTRAINT fk_aud_obj_id_typ_cd_tbl FOREIGN KEY (id_typ_cd_id) REFERENCES aud_cd_tbl(cd_id),
	CONSTRAINT fk_aud_obj_aud_tbl FOREIGN KEY (aud_id) REFERENCES aud_tbl(aud_id)
);

-- INDEX: LOOKUP OBJECT BY AUDIT ID
CREATE INDEX aud_obj_aud_idx ON aud_obj_tbl(aud_id);

-- AUDITABLE OBJECT SPECIFICATION TABLE
CREATE TABLE aud_obj_spec_tbl (
	spec_id UUID NOT NULL DEFAULT uuid_generate_v1(),
	obj_id UUID NOT NULL, -- THE OBJECT TO WHICH THE SPEC APPLIES
	spec TEXT NOT NULL, -- THE SPECIFICATION
	spec_typ CHAR(1) CHECK (spec_typ IN ('N','Q')),
	CONSTRAINT pk_aud_obj_spec_tbl PRIMARY KEY (spec_id),
	CONSTRAINT Fk_aud_obj_spec_obj_tbl FOREIGN KEY (obj_id) REFERENCES aud_obj_tbl(obj_id)
);

-- TABLE: AUDIT OBJECT DETAILS TABLE
-- TRACKS ADDITIONAL DETAIL ABOUT AN AUDITABLE OBJECT
CREATE TABLE aud_obj_dtl_tbl
(
	dtl_id UUID NOT NULL DEFAULT uuid_generate_v1(), -- A UNIQUE IDENTIFIER FOR THE OBJECT DETAIL LINE
	obj_id UUID NOT NULL, -- IDENTIFIES THE AUDITABLE OBJECT TO WHICH THE DETAIL BELONGS
	dtl_typ VARCHAR(256) NOT NULL, -- IDENTIFIES THE TYPE OF DETAIL
	dtl_val BYTEA NOT NULL, -- IDENTIFIES THE ADDITIONAL DETAIL DATA
	CONSTRAINT pk_aud_obj_dtl_tbl PRIMARY KEY (dtl_id),
	CONSTRAINT fk_aud_obj_dtl_obj_tbl FOREIGN KEY (obj_id) REFERENCES aud_obj_tbl(obj_id)
);

-- INDEX: LOOKUP OBJECT DETAIL BY OBJECT ID
CREATE INDEX aud_obj_dtl_obj_idx ON aud_obj_dtl_tbl(obj_id);

-- TABLE: AUDIT ERROR
-- STORES AUDIT ERRORS 
CREATE TABLE aud_dtl_tbl
(
	dtl_id UUID NOT NULL DEFAULT uuid_generate_v1(), -- A UNIQUE IDENTIFIER FOR THE ERROR
	ses_id UUID NOT NULL, -- THE SESSION IN WHICH THE AUDIT WAS COLLECTED
	level NUMERIC(2) NOT NULL, -- THE LEVEL OF THE DETAIL
	msg TEXT NOT NULL, -- THE ERROR MESSAGE
	aud_id UUID, -- THE MESSAGE ID IF APPLICABLE
	stack TEXT, -- THE STACK TRACE OF ANY EXCEPTION WHICH CAUSED THE ERROR
	crt_utc TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
	caus_by_id UUID, -- THE CAUSE OF THIS ERROR (IF APPLICABLE)
	sts_cd_id INTEGER NOT NULL DEFAULT 0,
	CONSTRAINT pk_aud_dtl_tbl PRIMARY KEY (err_id), 
	CONSTRAINT fk_aud_dtl_ses_tbl FOREIGN KEY (ses_id) REFERENCES aud_ses_tbl(ses_id),
	CONSTRAINT fk_aud_dtl_caus_tbl FOREIGN KEY (caus_by_id) REFERENCES aud_err_tbl(err_id),
	CONSTRAINT fk_aud_dtl_sts_tbl FOREIGN KEY (sts_cd_id) REFERENCES aud_sts_cdtbl(cd_id)
);

export type Status = 'Activo' | 'Inactivo' | 'Suspendido';

export interface User {
  id: string;
  fullName: string;
  login: string;
  email: string;
  roles: string[];
  companyId: string;
  salesPointId?: string;
  salesPointName?: string;
  supervisedSalesPointIds: string[];
  supervisedSalesPointNames: string[];
}

export interface Company {
  id: string;
  name: string;
  subdomain: string;
  customDomain?: string;
  logoDataUrl?: string;
  active: boolean;
}

export interface Customer {
  id: string;
  name: string;
  firstNames: string;
  lastNames: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  secondLastName?: string;
  identificationType?: number;
  identificationNumber?: string;
  companyName?: string;
  email: string;
  phoneCountryCode?: string;
  phone?: string;
  address?: string;
  city?: string;
  birthDate?: string;
  occupation?: string;
  status: number;
  tags?: string;
  notes?: string;
}

export interface Lead {
  id: string;
  name: string;
  firstNames: string;
  lastNames: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  secondLastName?: string;
  email: string;
  phone?: string;
  source: string;
  rating: number;
  converted: boolean;
}

export interface DealStage {
  id: string;
  name: string;
  order: number;
  defaultProbability: number;
  active: boolean;
}

export interface Deal {
  id: string;
  title: string;
  customerId?: string;
  stageId: string;
  value: number;
  closeProbability: number;
  estimatedCloseDate: string;
  status: number;
}

export interface Activity {
  id: string;
  title: string;
  description?: string;
  type: number;
  status: number;
  scheduledAt: string;
  reminderAt?: string;
  customerId?: string;
  dealId?: string;
  assignedUserId?: string;
  customerName?: string;
  dealTitle?: string;
}

export interface ProductPhoto {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  isQuoteDefault: boolean;
  dataUrl: string;
}

export interface ProductCategory {
  id: string;
  name: string;
  description?: string;
  quoteAsBundle: boolean;
  active: boolean;
}

export interface Product {
  id: string;
  name: string;
  category: string;
  brand: string;
  model: string;
  line?: string;
  version?: string;
  reference: string;
  description?: string;
  engineCc?: number;
  year?: number;
  color?: string;
  price: number;
  soat: number;
  registrationFee: number;
  taxes: number;
  technicalSheet?: string;
  priceValidFrom?: string;
  active: boolean;
  photos: ProductPhoto[];
  salesPointPrices: ProductSalesPointPrice[];
}

export interface ProductSalesPointPrice {
  salesPointId: string;
  salesPointName?: string;
  price: number;
  priceValidFrom?: string;
  active: boolean;
}

export interface ExternalInventoryItem {
  warehouseCode: string;
  warehouseName: string;
  code: string;
  name: string;
  presentation?: string;
  serialNumber?: string;
  engineNumber?: string;
  chassisNumber?: string;
  quantity: number;
  productId?: string;
  productName?: string;
  productPrice?: number;
  isInCatalog: boolean;
}

export interface CommercialInventory {
  id: string;
  productId: string;
  productName: string;
  salesPointId: string;
  salesPointName: string;
  vin?: string;
  chassisNumber?: string;
  engineNumber?: string;
  plate?: string;
  color?: string;
  isUsed: boolean;
  mileage?: number;
  status: number;
  reservedCustomerId?: string;
  reservedCustomerName?: string;
  reservedQuoteId?: string;
  reservedQuoteNumber?: string;
  reservedCreditApplicationId?: string;
  reservedCreditApplicationNumber?: string;
  reservedAt?: string;
  reservationExpiresAt?: string;
  reservationExpired: boolean;
  notes?: string;
}

export interface CommercialInventorySummary {
  productId: string;
  productName: string;
  salesPointId: string;
  salesPointName: string;
  available: number;
  reserved: number;
  sold: number;
  used: number;
  unavailable: number;
}

export interface FinancialSettings {
  id: string;
  minimumWage: number;
  consumerAnnualRate: number;
  lowAmountAnnualRate: number;
  factorMonthlyRate: number;
  maxTermMonths: number;
  paymentRounding: number;
  useMontelibanoTable: boolean;
  active: boolean;
}

export interface SalesPoint {
  id: string;
  name: string;
  code: string;
  city: string;
  address?: string;
  phone?: string;
  mainBrand: string;
  brandLogoDataUrl?: string;
  factorMonthlyRate: number;
  maxTermMonths: number;
  quoteValidityDays: number;
  deliveryMode: string;
  soatDays: number;
  registrationDays: number;
  soatProvider?: string;
  registrationAgent?: string;
  commercialTerms?: string;
  active: boolean;
}

export interface RequirementDocument {
  id: string;
  type: number;
  name: string;
  description?: string;
  required: boolean;
  order: number;
}

export interface RequirementProfile {
  id: string;
  name: string;
  code: string;
  description?: string;
  isCash: boolean;
  active: boolean;
  documents: RequirementDocument[];
}

export interface Promotion {
  id: string;
  name: string;
  code: string;
  discountType: string;
  discountValue: number;
  productId?: string;
  productName?: string;
  brand?: string;
  color?: string;
  salesPointId?: string;
  salesPointName?: string;
  validFrom: string;
  validUntil: string;
  active: boolean;
}

export interface QuoteSimulationResult {
  downPayment: number;
  insurance: number;
  administrativeFees: number;
  termMonths: number;
  monthlyInterestRate: number;
  promotionId?: string;
  promotionName?: string;
  promotionDiscount: number;
  discountedProductPrice: number;
  financedAmount: number;
  estimatedMonthlyPayment: number;
  estimatedTotalPayment: number;
  creditType: string;
  usedCompanyFinancialSettings: boolean;
}

export interface QuoteItem {
  id: string;
  productId: string;
  productName: string;
  productPrice: number;
  promotionId?: string;
  promotionName?: string;
  promotionDiscount: number;
  discountedProductPrice: number;
  downPayment: number;
  insurance: number;
  administrativeFees: number;
  termMonths: number;
  monthlyInterestRate: number;
  financedAmount: number;
  estimatedMonthlyPayment: number;
  estimatedTotalPayment: number;
  creditType?: string;
  usedCompanyFinancialSettings: boolean;
  order: number;
}

export interface Quote {
  id: string;
  number: string;
  identificationType: number;
  identificationNumber?: string;
  customerFirstNames: string;
  customerLastNames: string;
  customerFirstName: string;
  customerMiddleName?: string;
  customerLastName: string;
  customerSecondLastName?: string;
  customerId: string;
  productId: string;
  productName: string;
  productTechnicalSheet?: string;
  salesPointId?: string;
  salesPointName?: string;
  salesPointBrand?: string;
  salesPointDeliveryMode?: string;
  salesPointCommercialTerms?: string;
  requirementProfileId?: string;
  requirementProfileName?: string;
  promotionId?: string;
  promotionName?: string;
  promotionDiscount: number;
  discountedProductPrice: number;
  productPrice: number;
  downPayment: number;
  insurance: number;
  administrativeFees: number;
  termMonths: number;
  monthlyInterestRate: number;
  financedAmount: number;
  estimatedMonthlyPayment: number;
  estimatedTotalPayment: number;
  creditType?: string;
  usedCompanyFinancialSettings: boolean;
  quoteDate: string;
  validUntil: string;
  notes?: string;
  items: QuoteItem[];
}

export interface ColombianIdentityLookup {
  documentNumber: string;
  documentType?: string;
  firstName?: string;
  middleName?: string;
  lastName?: string;
  secondLastName?: string;
  fullName?: string;
  dateOfBirth?: string;
  expeditionDate?: string;
  expeditionCity?: string;
  expeditionDepartment?: string;
  gender?: string;
  isAlive?: boolean;
  source: 'database' | 'verifik' | string;
}

export interface CreditDocument {
  id: string;
  customerId?: string;
  type: number;
  name: string;
  status: number;
  receivedAt?: string;
  expiresAt?: string;
  notes?: string;
  rejectedAt?: string;
  rejectionReason?: string;
  validatedAt?: string;
  validatedBy?: string;
  isExpired: boolean;
  daysToExpire?: number;
  hasFile: boolean;
  fileName?: string;
  contentType?: string;
  sizeBytes?: number;
  uploadedAt?: string;
}

export interface CreditApplication {
  id: string;
  number: string;
  customerId: string;
  customerName: string;
  productId: string;
  productName: string;
  quoteId?: string;
  dealId?: string;
  requirementProfileId?: string;
  requirementProfileName?: string;
  identificationType: number;
  identificationNumber: string;
  birthDate?: string;
  mobile: string;
  address?: string;
  city?: string;
  occupation?: string;
  monthlyIncome: number;
  downPayment: number;
  termMonths: number;
  motorcycleValue: number;
  coDebtorName?: string;
  coDebtorIdentification?: string;
  coDebtorMobile?: string;
  coDebtorRelationship?: string;
  coDebtorMonthlyIncome?: number;
  reference1Name?: string;
  reference1Mobile?: string;
  reference1Relationship?: string;
  reference2Name?: string;
  reference2Mobile?: string;
  reference2Relationship?: string;
  status: number;
  notes?: string;
  submittedAt?: string;
  reviewStartedAt?: string;
  step0ReviewedAt?: string;
  runtChecked: boolean;
  simitChecked: boolean;
  identityValidated: boolean;
  step0User?: string;
  step0Notes?: string;
  analystApprovedAmount?: number;
  approvedDownPayment?: number;
  approvedTermMonths?: number;
  approvedMonthlyPayment?: number;
  requiresCoDebtorForApproval: boolean;
  finalConditions?: string;
  studyResult?: string;
  approvedAt?: string;
  rejectedAt?: string;
  disbursedAt?: string;
  decisionUser?: string;
  decisionNotes?: string;
  documents: CreditDocument[];
}

export interface MotorcycleDelivery {
  id: string;
  number: string;
  creditApplicationId: string;
  creditApplicationNumber: string;
  customerId: string;
  customerName: string;
  productId: string;
  productName: string;
  deliveryDate: string;
  responsibleAdvisor?: string;
  vin?: string;
  chassisNumber?: string;
  engineNumber?: string;
  plate?: string;
  deliveryMileage?: number;
  helmetDelivered: boolean;
  soatDelivered: boolean;
  registrationDelivered: boolean;
  warrantyManualDelivered: boolean;
  deliveryCertificateSigned: boolean;
  preDeliveryChecklistCompleted: boolean;
  deliveryProtocol?: string;
  deliveryPhotoDataUrl?: string;
  deliveryPhotoFileName?: string;
  firstServiceScheduledAt?: string;
  firstServiceActivityId?: string;
  status: number;
  notes?: string;
}

export interface CollectionOrderDetail {
  id: string;
  type: number;
  concept: string;
  amount: number;
}

export interface CollectionOrder {
  id: string;
  number: string;
  creditApplicationId: string;
  creditApplicationNumber: string;
  customerId: string;
  customerName: string;
  issueDate: string;
  dueDate: string;
  vehicleAmount: number;
  documentsAmount: number;
  advanceAmount: number;
  total: number;
  paidAmount: number;
  balance: number;
  paidAt?: string;
  status: number;
  notes?: string;
  details: CollectionOrderDetail[];
}

export interface Procedure {
  id: string;
  number: string;
  creditApplicationId: string;
  creditApplicationNumber: string;
  customerId: string;
  customerName: string;
  customerMobile: string;
  productId: string;
  productName: string;
  salesPointId?: string;
  salesPointName?: string;
  type: number;
  status: number;
  startDate: string;
  estimatedDate: string;
  completedAt?: string;
  responsible?: string;
  thirdParty?: string;
  notifyCustomer: boolean;
  customerNotifiedAt?: string;
  isOverdue: boolean;
  whatsappMessage: string;
  notes?: string;
}

export interface Customer360 {
  customer: Customer;
  quotes: Quote[];
  creditApplications: CreditApplication[];
  deals: Deal[];
  activities: Activity[];
  timeline: CustomerTimelineItem[];
}

export interface CustomerAiAnalysis {
  summary: string;
  pendingItems: string[];
  riskLevel: string;
  priority: string;
  nextBestAction: string;
  whatsappMessage: string;
  signals: string[];
}

export interface CustomerTimelineItem {
  occurredAt: string;
  type: string;
  title: string;
  description: string;
  tone: 'success' | 'warning' | 'error' | 'info' | 'default';
  relatedId?: string;
}

export interface Dashboard {
  openPipelineValue: number;
  weightedPipelineValue: number;
  activeCustomers: number;
  openLeads: number;
  pendingActivities: number;
  overdueActivities: number;
  todayActivities: number;
  recentActivities: { title: string; scheduledAt: string; status: number }[];
  alerts: CommercialAlert[];
}

export interface CommercialAlert {
  type: string;
  severity: 'error' | 'warning' | 'info' | 'success' | string;
  title: string;
  description: string;
  createdAt: string;
  actionUrl?: string;
}

export interface CommercialReports {
  summary: CommercialReportSummary;
  salesBySeller: SalesBySeller[];
  quotesByStatus: QuotesByStatus[];
  creditsByStatus: CreditsByStatus[];
  topQuotedProducts: TopQuotedProduct[];
}

export interface CommercialReportSummary {
  totalQuotes: number;
  quotesConvertedToCredit: number;
  quoteToCreditConversionRate: number;
  approvedCredits: number;
  rejectedCredits: number;
  creditApprovalRate: number;
  approvedCreditAmount: number;
}

export interface SalesBySeller {
  seller: string;
  quotes: number;
  approvedCredits: number;
  approvedAmount: number;
}

export interface QuotesByStatus {
  status: string;
  count: number;
  amount: number;
}

export interface CreditsByStatus {
  status: string;
  count: number;
  amount: number;
}

export interface TopQuotedProduct {
  productId: string;
  productName: string;
  brand: string;
  model: string;
  quoteCount: number;
  quotedAmount: number;
}

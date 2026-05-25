export type Status = 'Activo' | 'Inactivo' | 'Suspendido';

export interface User {
  id: string;
  fullName: string;
  email: string;
  roles: string[];
  companyId: string;
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

export interface Product {
  id: string;
  name: string;
  category: string;
  brand: string;
  model: string;
  reference: string;
  description?: string;
  engineCc?: number;
  year?: number;
  color?: string;
  price: number;
  active: boolean;
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
  productPrice: number;
  downPayment: number;
  termMonths: number;
  monthlyInterestRate: number;
  financedAmount: number;
  estimatedMonthlyPayment: number;
  estimatedTotalPayment: number;
  quoteDate: string;
  validUntil: string;
  notes?: string;
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
  type: number;
  name: string;
  status: number;
  receivedAt?: string;
  notes?: string;
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
  status: number;
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

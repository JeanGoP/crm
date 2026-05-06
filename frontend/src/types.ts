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
  active: boolean;
}

export interface Customer {
  id: string;
  name: string;
  firstNames: string;
  lastNames: string;
  companyName?: string;
  email: string;
  phone?: string;
  status: number;
  tags?: string;
}

export interface Lead {
  id: string;
  name: string;
  firstNames: string;
  lastNames: string;
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
  brand: string;
  model: string;
  reference: string;
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

export interface Customer360 {
  customer: Customer;
  quotes: Quote[];
  creditApplications: CreditApplication[];
  deals: Deal[];
  activities: Activity[];
  timeline: CustomerTimelineItem[];
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

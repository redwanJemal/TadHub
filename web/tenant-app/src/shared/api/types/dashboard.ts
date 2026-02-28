/**
 * Types for the BFF Dashboard endpoint
 */

export interface DashboardKpis {
  activeWorkers: number;
  totalWorkers: number;
  activeContracts: number;
  totalContracts: number;
  pendingCandidates: number;
  totalCandidates: number;
  activeClients: number;
  totalClients: number;
}

export interface DashboardCompliance {
  totalDocuments: number;
  valid: number;
  expiringSoon: number;
  expired: number;
  pending: number;
  complianceRate: number;
}

export interface DashboardActivityItem {
  id: string;
  eventName: string;
  entityName: string | null;
  createdAt: string;
}

export interface DashboardSummary {
  kpis: DashboardKpis;
  compliance: DashboardCompliance;
  recentActivity: DashboardActivityItem[];
  generatedAt: string;
}

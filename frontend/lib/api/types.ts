export type User = { id: string; tenantId: string; name: string; email: string; role: string };
export type AuthResponse = { accessToken: string; refreshToken: string; accessTokenExpiresAt: string; user: User };
export type IntegrationStatus = { provider: string; status: string; displayPhoneNumber: string; lastWebhookAt?: string; lastSuccessfulSendAt?: string; lastFailureAt?: string; failureReason?: string };
export type Clinic = { id: string; legalName: string; tradeName: string; timeZone: string; status: string };
export type Unit = { id: string; name: string; address: string; phone: string; status: string };
export type Specialty = { id: string; name: string; description?: string; status: string };
export type Professional = { id: string; clinicUnitId: string; name: string; email: string; phone: string; registrationNumber: string; status: string; specialtyIds: string[] };
export type Patient = { id: string; name: string; phone: string; email?: string; birthDate?: string; consentStatus: string };

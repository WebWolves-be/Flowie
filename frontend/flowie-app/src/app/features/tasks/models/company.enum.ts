export enum Company {
  Immoseed = 'Immoseed',
  NovaraRealEstate = 'NovaraRealEstate'
}

// Helper map if you want shorter display labels later
export const CompanyDisplayLabels: Record<Company, string> = {
  [Company.Immoseed]: 'Immoseed',
  [Company.NovaraRealEstate]: 'Novara' // Shorter label for UI if desired
};
export enum Company {
  Immoseed = 'Immoseed',
  NovaraRealEstate = 'NovaraRealEstate',
  Algemeen = 'Algemeen'
}

export const CompanyDisplayLabels: Record<Company, string> = {
  [Company.Immoseed]: 'Immoseed',
  [Company.NovaraRealEstate]: 'Novara',
  [Company.Algemeen]: 'Algemeen'
};
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from 'react-oidc-context';
import { Check, Building2, User, Users, Sparkles } from 'lucide-react';
import { toast } from 'sonner';
import { apiClient } from '@/shared/api';
import { setTenantId } from '@/features/auth/AuthProvider';

type Step = 'workspace' | 'profile' | 'team' | 'complete';

export function OnboardingPage() {
  const { t } = useTranslation('onboarding');
  const navigate = useNavigate();
  const auth = useAuth();
  const [currentStep, setCurrentStep] = useState<Step>('workspace');
  const [isLoading, setIsLoading] = useState(false);
  
  const [workspaceData, setWorkspaceData] = useState({
    name: '',
    slug: '',
    industry: '',
    size: '',
  });

  const [profileData, setProfileData] = useState({
    jobTitle: '',
    phone: '',
  });

  const [teamEmails, setTeamEmails] = useState('');

  const steps: { id: Step; icon: React.ElementType }[] = [
    { id: 'workspace', icon: Building2 },
    { id: 'profile', icon: User },
    { id: 'team', icon: Users },
    { id: 'complete', icon: Sparkles },
  ];

  const currentStepIndex = steps.findIndex(s => s.id === currentStep);

  const handleNext = async () => {
    if (currentStep === 'workspace') {
      if (!workspaceData.name) {
        toast.error(t('validation.required'));
        return;
      }
      setCurrentStep('profile');
    } else if (currentStep === 'profile') {
      setCurrentStep('team');
    } else if (currentStep === 'team') {
      setIsLoading(true);
      try {
        // Call the onboarding API endpoint
        const response = await apiClient.post<{ tenantId: string }>('/onboarding', {
          tenantName: workspaceData.name,
          tenantSlug: workspaceData.slug || workspaceData.name.toLowerCase().replace(/[^a-z0-9]+/g, '-'),
          displayName: auth.user?.profile?.name || `${auth.user?.profile?.given_name || ''} ${auth.user?.profile?.family_name || ''}`.trim(),
          jobTitle: profileData.jobTitle || undefined,
          phoneNumber: profileData.phone || undefined,
          teamSize: workspaceData.size || undefined,
          inviteEmails: teamEmails ? teamEmails.split(/[,\n]/).map(e => e.trim()).filter(Boolean) : undefined,
        });
        
        // Set the tenant ID for subsequent API calls
        if (response.tenantId) {
          setTenantId(response.tenantId);
        }
        
        setCurrentStep('complete');
      } catch (error) {
        console.error('Onboarding error:', error);
        toast.error('Failed to complete onboarding');
      } finally {
        setIsLoading(false);
      }
    }
  };

  const handleComplete = () => {
    navigate('/dashboard');
  };

  const generateSlug = (name: string) => {
    return name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-primary/5 via-background to-primary/10">
      <div className="mx-auto max-w-2xl px-4 py-12">
        {/* Header */}
        <div className="text-center mb-12">
          <h1 className="text-3xl font-bold text-foreground">{t('title')}</h1>
          <p className="mt-2 text-muted-foreground">{t('subtitle')}</p>
        </div>

        {/* Progress */}
        <div className="flex justify-center mb-12">
          <div className="flex items-center gap-4">
            {steps.map((step, index) => (
              <div key={step.id} className="flex items-center">
                <div
                  className={`flex h-10 w-10 items-center justify-center rounded-full border-2 transition-colors ${
                    index < currentStepIndex
                      ? 'border-primary bg-primary text-primary-foreground'
                      : index === currentStepIndex
                      ? 'border-primary text-primary'
                      : 'border-muted text-muted-foreground'
                  }`}
                >
                  {index < currentStepIndex ? (
                    <Check className="h-5 w-5" />
                  ) : (
                    <step.icon className="h-5 w-5" />
                  )}
                </div>
                {index < steps.length - 1 && (
                  <div
                    className={`h-0.5 w-12 mx-2 ${
                      index < currentStepIndex ? 'bg-primary' : 'bg-muted'
                    }`}
                  />
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Content */}
        <div className="rounded-2xl bg-card border border-border p-8 shadow-lg">
          {currentStep === 'workspace' && (
            <div className="space-y-6">
              <div>
                <h2 className="text-xl font-bold text-foreground">
                  {t('workspace.title')}
                </h2>
                <p className="text-muted-foreground mt-1">
                  {t('workspace.subtitle')}
                </p>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-foreground mb-2">
                    {t('workspace.name')} *
                  </label>
                  <input
                    type="text"
                    value={workspaceData.name}
                    onChange={(e) => {
                      setWorkspaceData({
                        ...workspaceData,
                        name: e.target.value,
                        slug: generateSlug(e.target.value),
                      });
                    }}
                    placeholder={t('workspace.namePlaceholder')}
                    className="w-full rounded-lg border border-input bg-background px-4 py-3 text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                  />
                  <p className="text-sm text-muted-foreground mt-1">
                    {t('workspace.nameHint')}
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-foreground mb-2">
                    {t('workspace.slug')}
                  </label>
                  <div className="flex items-center gap-2">
                    <span className="text-muted-foreground">workspace/</span>
                    <input
                      type="text"
                      value={workspaceData.slug}
                      onChange={(e) => setWorkspaceData({ ...workspaceData, slug: e.target.value })}
                      placeholder={t('workspace.slugPlaceholder')}
                      className="flex-1 rounded-lg border border-input bg-background px-4 py-3 text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-foreground mb-2">
                    {t('workspace.size')}
                  </label>
                  <select
                    value={workspaceData.size}
                    onChange={(e) => setWorkspaceData({ ...workspaceData, size: e.target.value })}
                    className="w-full rounded-lg border border-input bg-background px-4 py-3 text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                  >
                    <option value="">{t('workspace.sizePlaceholder')}</option>
                    <option value="solo">{t('teamSizes.solo')}</option>
                    <option value="small">{t('teamSizes.small')}</option>
                    <option value="medium">{t('teamSizes.medium')}</option>
                    <option value="large">{t('teamSizes.large')}</option>
                    <option value="enterprise">{t('teamSizes.enterprise')}</option>
                  </select>
                </div>
              </div>
            </div>
          )}

          {currentStep === 'profile' && (
            <div className="space-y-6">
              <div>
                <h2 className="text-xl font-bold text-foreground">
                  {t('profile.title')}
                </h2>
                <p className="text-muted-foreground mt-1">
                  {t('profile.subtitle')}
                </p>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-foreground mb-2">
                    {t('profile.jobTitle')}
                  </label>
                  <input
                    type="text"
                    value={profileData.jobTitle}
                    onChange={(e) => setProfileData({ ...profileData, jobTitle: e.target.value })}
                    placeholder={t('profile.jobTitlePlaceholder')}
                    className="w-full rounded-lg border border-input bg-background px-4 py-3 text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-foreground mb-2">
                    {t('profile.phone')}
                  </label>
                  <input
                    type="tel"
                    value={profileData.phone}
                    onChange={(e) => setProfileData({ ...profileData, phone: e.target.value })}
                    placeholder={t('profile.phonePlaceholder')}
                    className="w-full rounded-lg border border-input bg-background px-4 py-3 text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                  />
                </div>
              </div>
            </div>
          )}

          {currentStep === 'team' && (
            <div className="space-y-6">
              <div>
                <h2 className="text-xl font-bold text-foreground">
                  {t('team.title')}
                </h2>
                <p className="text-muted-foreground mt-1">
                  {t('team.subtitle')}
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium text-foreground mb-2">
                  {t('team.inviteEmails')}
                </label>
                <textarea
                  value={teamEmails}
                  onChange={(e) => setTeamEmails(e.target.value)}
                  placeholder={t('team.inviteEmailsPlaceholder')}
                  rows={4}
                  className="w-full rounded-lg border border-input bg-background px-4 py-3 text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 resize-none"
                />
                <p className="text-sm text-muted-foreground mt-1">
                  {t('team.inviteEmailsHint')}
                </p>
              </div>
            </div>
          )}

          {currentStep === 'complete' && (
            <div className="text-center py-8">
              <div className="mx-auto w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center mb-6">
                <Sparkles className="h-8 w-8 text-primary" />
              </div>
              <h2 className="text-2xl font-bold text-foreground">
                {t('complete.title')}
              </h2>
              <p className="text-muted-foreground mt-2">
                {t('complete.subtitle')}
              </p>
            </div>
          )}

          {/* Actions */}
          <div className="flex justify-between mt-8 pt-6 border-t border-border">
            {currentStep !== 'complete' && currentStep !== 'workspace' && (
              <button
                onClick={() => {
                  const prevStep = steps[currentStepIndex - 1];
                  if (prevStep) setCurrentStep(prevStep.id);
                }}
                className="px-6 py-2 text-muted-foreground hover:text-foreground transition-colors"
              >
                {t('back', { ns: 'common' })}
              </button>
            )}
            
            {currentStep === 'workspace' && <div />}

            {currentStep !== 'complete' ? (
              <button
                onClick={handleNext}
                disabled={isLoading}
                className="rounded-lg bg-primary px-6 py-2 font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50 transition-colors"
              >
                {currentStep === 'team' 
                  ? (isLoading ? '...' : t('finish', { ns: 'common' }))
                  : t('next', { ns: 'common' })
                }
              </button>
            ) : (
              <button
                onClick={handleComplete}
                className="w-full rounded-lg bg-primary px-6 py-3 font-medium text-primary-foreground hover:bg-primary/90 transition-colors"
              >
                {t('complete.goToDashboard')}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

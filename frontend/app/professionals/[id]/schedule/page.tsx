import ProfessionalScheduleClient from "./schedule-client";

export default async function ProfessionalSchedulePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  return <ProfessionalScheduleClient professionalId={id} />;
}

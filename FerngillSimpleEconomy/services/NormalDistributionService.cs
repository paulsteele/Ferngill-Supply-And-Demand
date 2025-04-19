using System;
using fse.core.models;
using MathNet.Numerics.Distributions;

namespace fse.core.services;

public interface INormalDistributionService
{
	void Reset();
	double SampleSupply();
	double SampleSeasonlessDelta();
	double SampleInSeasonDelta();
	double SampleOutOfSeasonDelta();
}

public class NormalDistributionService : INormalDistributionService
{
	private Normal _supplyNormal = new();
	private Normal _deltaNormal = new();
	private Normal _inSeasonNormal = new();
	private Normal _outOfSeasonNormal = new();
	private static int MeanSupply => (ConfigModel.MinSupply + ConfigModel.Instance.MaxCalculatedSupply) / 2;
	private static int MeanDelta => (ConfigModel.Instance.MinDelta + ConfigModel.Instance.MaxDelta) / 2;

	public void Reset()
	{
		var rand = new Random();
		_supplyNormal = new Normal(MeanSupply, ConfigModel.Instance.StdDevSupply, rand);
		_deltaNormal = new Normal(MeanDelta, ConfigModel.Instance.StdDevDelta, rand);
		_inSeasonNormal = new Normal(MeanDelta, ConfigModel.Instance.StdDevDeltaInSeason, rand);
		_outOfSeasonNormal = new Normal(MeanDelta, ConfigModel.Instance.StdDevDeltaOutOfSeason, rand);
	}

	public double SampleSupply() => _supplyNormal.Sample();
	public double SampleSeasonlessDelta() => _deltaNormal.Sample();
	public double SampleInSeasonDelta() => _inSeasonNormal.Sample();
	public double SampleOutOfSeasonDelta() => _outOfSeasonNormal.Sample();
}